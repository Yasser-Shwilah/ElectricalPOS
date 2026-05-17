using ElectricalPOS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace ElectricalPOS.Services
{
    public class BackupService
    {
        private readonly string _defaultBackupFolder;
        private readonly string _connectionString;

        public BackupService()
        {
            // Set default backup folder to C:\SQLBackups for SQL Server access
            _defaultBackupFolder = @"C:\SQLBackups";

            // Create backup folder if it doesn't exist
            if (!Directory.Exists(_defaultBackupFolder))
            {
                try
                {
                    Directory.CreateDirectory(_defaultBackupFolder);
                }
                catch (Exception ex)
                {
                    // If C:\SQLBackups fails, try using the application directory
                    _defaultBackupFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
                    if (!Directory.Exists(_defaultBackupFolder))
                    {
                        Directory.CreateDirectory(_defaultBackupFolder);
                    }
                }
            }

            // Get connection string from context - use . instead of localhost for local SQL Server
            _connectionString = @"Server=.;Database=ElectricalPOS_DB;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        /// <summary>
        /// Creates a full database backup to the specified location
        /// </summary>
        public async Task<(bool Success, string Message, string? FilePath)> CreateBackupAsync(
            string? customPath = null,
            string backupType = "Manual",
            string? description = null,
            string? createdBy = null,
            bool compressBackup = true)
        {
            SqlConnection? sqlConnection = null;
            try
            {
                var backupFolder = customPath ?? _defaultBackupFolder;
                
                // Create backup folder if it doesn't exist
                if (!Directory.Exists(backupFolder))
                {
                    try
                    {
                        Directory.CreateDirectory(backupFolder);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"فشل إنشاء مجلد النسخ الاحتياطي: {ex.Message}", null);
                    }
                }

                // Check if we have write permissions to the backup folder
                try
                {
                    var testFile = Path.Combine(backupFolder, $"test_write_{Guid.NewGuid()}.tmp");
                    File.WriteAllText(testFile, "test");
                    File.Delete(testFile);
                }
                catch (UnauthorizedAccessException)
                {
                    // Try alternative folder
                    backupFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
                    if (!Directory.Exists(backupFolder))
                    {
                        Directory.CreateDirectory(backupFolder);
                    }
                }

                // Generate backup file name with timestamp
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var databaseName = "ElectricalPOS_DB";
                var backupFileName = $"{databaseName}_Backup_{timestamp}.bak";
                var backupFilePath = Path.Combine(backupFolder, backupFileName);

                // First verify the database exists
                sqlConnection = new SqlConnection(_connectionString);
                await sqlConnection.OpenAsync();

                // Check if database exists
                using (var checkCmd = new SqlCommand("SELECT COUNT(*) FROM sys.databases WHERE name = 'ElectricalPOS_DB'", sqlConnection))
                {
                    var dbExists = (int)await checkCmd.ExecuteScalarAsync();
                    if (dbExists == 0)
                    {
                        return (false, "قاعدة البيانات 'ElectricalPOS_DB' غير موجودة!", null);
                    }
                }

                // Use T-SQL BACKUP command instead of SMO for more reliability
                var backupQuery = @" 
                    BACKUP DATABASE [ElectricalPOS_DB] 
                    TO DISK = @BackupFilePath 
                    WITH FORMAT, INIT, NAME = N'ElectricalPOS_DB-Full Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10";

                using (var backupCmd = new SqlCommand(backupQuery, sqlConnection))
                {
                    backupCmd.Parameters.AddWithValue("@BackupFilePath", backupFilePath);
                    await backupCmd.ExecuteNonQueryAsync();
                }

                // Close connection before file operations
                await sqlConnection.CloseAsync();
                sqlConnection = null;

                // Check if backup file was created
                if (!File.Exists(backupFilePath))
                {
                    return (false, "لم يتم إنشاء ملف النسخ الاحتياطي!", null);
                }

                // Get file size
                var fileInfo = new FileInfo(backupFilePath);
                var fileSize = fileInfo.Length;

                // If compression is requested, compress the backup file
                string? compressedPath = null;
                if (compressBackup)
                {
                    var zipFileName = Path.ChangeExtension(backupFilePath, ".zip");
                    if (File.Exists(backupFilePath))
                    {
                        // Delete the old zip if exists
                        if (File.Exists(zipFileName))
                        {
                            File.Delete(zipFileName);
                        }
                        
                        try
                        {
                            using (var zipArchive = ZipFile.Open(zipFileName, ZipArchiveMode.Create))
                            {
                                zipArchive.CreateEntryFromFile(backupFilePath, backupFileName);
                            }
                            
                            compressedPath = zipFileName;
                            fileSize = new FileInfo(zipFileName).Length;
                            
                            // Delete the original .bak file after compression
                            try { File.Delete(backupFilePath); } catch { }
                        }
                        catch (Exception ex)
                        {
                            // If compression fails, use the uncompressed backup
                            compressedPath = null;
                        }
                    }
                }

                // Save backup record to database
                var backupRecord = new Models.Backup
                {
                    FilePath = compressBackup && compressedPath != null ? compressedPath : backupFilePath,
                    FileName = compressBackup && compressedPath != null 
                        ? Path.GetFileName(compressedPath) 
                        : backupFileName,
                    FileSize = fileSize,
                    BackupType = backupType,
                    Status = "Completed",
                    Description = description ?? $"نسخ احتياطي تلقائي - {timestamp}",
                    CreatedAt = DateTime.Now,
                    CreatedBy = createdBy ?? "System",
                    CompressionType = compressBackup && compressedPath != null ? "ZIP" : "None",
                    IsEncrypted = false,
                    ExpiresAt = DateTime.Now.AddYears(1)
                };

                await SaveBackupRecordAsync(backupRecord);

                return (true, $"تم إنشاء النسخ الاحتياطي بنجاح!\nالمسار: {backupFilePath}\nالحجم: {FormatFileSize(fileSize)}", 
                    compressBackup && compressedPath != null ? compressedPath : backupFilePath);
            }
            catch (SqlException ex)
            {
                // Log the error and save failed backup record
                var backupRecord = new Models.Backup
                {
                    FilePath = "",
                    FileName = $"Failed_Backup_{DateTime.Now:yyyyMMdd_HHmmss}",
                    FileSize = 0,
                    BackupType = backupType,
                    Status = "Failed",
                    Description = description,
                    CreatedAt = DateTime.Now,
                    CreatedBy = createdBy ?? "System",
                    ErrorMessage = $"SQL Error: {ex.Message}"
                };

                await SaveBackupRecordAsync(backupRecord);

                return (false, $"فشل إنشاء النسخ الاحتياطي (خطأ في SQL): {ex.Message}", null);
            }
            catch (Exception ex)
            {
                // Log the error and save failed backup record
                var backupRecord = new Models.Backup
                {
                    FilePath = "",
                    FileName = $"Failed_Backup_{DateTime.Now:yyyyMMdd_HHmmss}",
                    FileSize = 0,
                    BackupType = backupType,
                    Status = "Failed",
                    Description = description,
                    CreatedAt = DateTime.Now,
                    CreatedBy = createdBy ?? "System",
                    ErrorMessage = ex.Message
                };

                await SaveBackupRecordAsync(backupRecord);

                return (false, $"فشل إنشاء النسخ الاحتياطي: {ex.Message}", null);
            }
            finally
            {
                if (sqlConnection != null)
                {
                    if (sqlConnection.State != System.Data.ConnectionState.Closed)
                        await sqlConnection.CloseAsync();
                    sqlConnection.Dispose();
                }
            }
        }

        /// <summary>
        /// Restores database from a backup file
        /// </summary>
        public async Task<(bool Success, string Message)> RestoreBackupAsync(string backupFilePath)
        {
            SqlConnection? sqlConnection = null;
            string? tempExtractPath = null;
            try
            {
                if (!File.Exists(backupFilePath))
                {
                    return (false, "ملف النسخ الاحتياطي غير موجود!");
                }

                // Check if it's a compressed file
                if (Path.GetExtension(backupFilePath).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    // Extract the zip file temporarily
                    tempExtractPath = Path.Combine(Path.GetTempPath(), "ElectricalPOS_Restore_" + Guid.NewGuid());
                    Directory.CreateDirectory(tempExtractPath);
                    
                    try
                    {
                        ZipFile.ExtractToDirectory(backupFilePath, tempExtractPath);
                        
                        // Find the .bak file
                        var bakFiles = Directory.GetFiles(tempExtractPath, "*.bak");
                        if (bakFiles.Length == 0)
                        {
                            return (false, "لم يتم العثور على ملف .bak في الملف المضغوط!");
                        }
                        
                        backupFilePath = bakFiles[0];
                    }
                    catch (Exception ex)
                    {
                        return (false, $"فشل فك ضغط الملف: {ex.Message}");
                    }
                }

                var databaseName = "ElectricalPOS_DB";
                
                // First, set database to single user mode and kill all connections
                sqlConnection = new SqlConnection(_connectionString);
                await sqlConnection.OpenAsync();

                // Kill all connections to the database
                using (var killCmd = new SqlCommand(@"
                    ALTER DATABASE [ElectricalPOS_DB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", 
                    sqlConnection))
                {
                    await killCmd.ExecuteNonQueryAsync();
                }

                // Perform restore using T-SQL
                var restoreQuery = @"
                    RESTORE DATABASE [ElectricalPOS_DB] 
                    FROM DISK = @BackupFilePath 
                    WITH REPLACE, RECOVERY, STATS = 10";

                using (var restoreCmd = new SqlCommand(restoreQuery, sqlConnection))
                {
                    restoreCmd.Parameters.AddWithValue("@BackupFilePath", backupFilePath);
                    await restoreCmd.ExecuteNonQueryAsync();
                }

                // Set database back to multi-user mode
                using (var multiUserCmd = new SqlCommand(@"
                    ALTER DATABASE [ElectricalPOS_DB] SET MULTI_USER", 
                    sqlConnection))
                {
                    await multiUserCmd.ExecuteNonQueryAsync();
                }

                await sqlConnection.CloseAsync();
                sqlConnection = null;

                // Cleanup temp folder
                if (tempExtractPath != null && Directory.Exists(tempExtractPath))
                {
                    try { Directory.Delete(tempExtractPath, true); } catch { }
                }

                return (true, "تم استعادة قاعدة البيانات بنجاح!");
            }
            catch (SqlException ex)
            {
                return (false, $"فشل استعادة قاعدة البيانات (خطأ في SQL): {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"فشل استعادة قاعدة البيانات: {ex.Message}");
            }
            finally
            {
                if (sqlConnection != null)
                {
                    if (sqlConnection.State != System.Data.ConnectionState.Closed)
                    {
                        // Try to set database back to multi-user mode in case of error
                        try
                        {
                            using var multiUserCmd = new SqlCommand(@"ALTER DATABASE [ElectricalPOS_DB] SET MULTI_USER", sqlConnection);
                            multiUserCmd.ExecuteNonQuery();
                        }
                        catch { }
                    }
                    await sqlConnection.CloseAsync();
                    sqlConnection.Dispose();
                }

                // Cleanup temp folder
                if (tempExtractPath != null && Directory.Exists(tempExtractPath))
                {
                    try { Directory.Delete(tempExtractPath, true); } catch { }
                }
            }
        }

        /// <summary>
        /// Gets all backup records from database
        /// </summary>
        public async Task<List<Models.Backup>> GetAllBackupsAsync()
        {
            using var context = new ElectricalPOSContext();
            return await context.Set<Models.Backup>()
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Gets backup history with pagination
        /// </summary>
        public async Task<List<Models.Backup>> GetBackupHistoryAsync(int page = 1, int pageSize = 20)
        {
            using var context = new ElectricalPOSContext();
            return await context.Set<Models.Backup>()
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Gets total backup count
        /// </summary>
        public async Task<int> GetTotalBackupCountAsync()
        {
            using var context = new ElectricalPOSContext();
            return await context.Set<Models.Backup>().CountAsync();
        }

        /// <summary>
        /// Deletes a backup record and optionally the file
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteBackupAsync(int backupId, bool deleteFile = true)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                var backup = await context.Set<Models.Backup>().FindAsync(backupId);
                
                if (backup == null)
                {
                    return (false, "سجل النسخ الاحتياطي غير موجود!");
                }

                // Delete physical file if requested and exists
                if (deleteFile && !string.IsNullOrEmpty(backup.FilePath) && File.Exists(backup.FilePath))
                {
                    File.Delete(backup.FilePath);
                }

                // Delete record from database
                context.Set<Models.Backup>().Remove(backup);
                await context.SaveChangesAsync();

                return (true, "تم حذف النسخ الاحتياطي بنجاح!");
            }
            catch (Exception ex)
            {
                return (false, $"فشل حذف النسخ الاحتياطي: {ex.Message}");
            }
        }

        /// <summary>
        /// Cleans up old backups based on retention policy
        /// </summary>
        public async Task<(int DeletedCount, string Message)> CleanupOldBackupsAsync(int retentionDays = 30)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                var cutoffDate = DateTime.Now.AddDays(-retentionDays);
                
                // Get old backups
                var oldBackups = await context.Set<Models.Backup>()
                    .Where(b => b.CreatedAt < cutoffDate)
                    .ToListAsync();

                int deletedCount = 0;
                foreach (var backup in oldBackups)
                {
                    // Delete physical file if exists
                    if (!string.IsNullOrEmpty(backup.FilePath) && File.Exists(backup.FilePath))
                    {
                        try
                        {
                            File.Delete(backup.FilePath);
                            deletedCount++;
                        }
                        catch
                        {
                            // Continue if file deletion fails
                        }
                    }
                }

                // Delete records from database
                context.Set<Models.Backup>().RemoveRange(oldBackups);
                await context.SaveChangesAsync();

                return (deletedCount, $"تم حذف {deletedCount} نسخة احتياطية قديمة!");
            }
            catch (Exception ex)
            {
                return (0, $"فشل تنظيف النسخ الاحتياطية القديمة: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets backup statistics
        /// </summary>
        public async Task<(int TotalBackups, long TotalSize, DateTime? LastBackup)> GetBackupStatisticsAsync()
        {
            using var context = new ElectricalPOSContext();
            var backups = await context.Set<Models.Backup>().ToListAsync();

            return (
                backups.Count,
                backups.Sum(b => b.FileSize),
                backups.OrderByDescending(b => b.CreatedAt).FirstOrDefault()?.CreatedAt
            );
        }

        /// <summary>
        /// Verifies a backup file is valid
        /// </summary>
        public async Task<(bool IsValid, string Message)> VerifyBackupAsync(string backupFilePath)
        {
            SqlConnection? sqlConnection = null;
            string? tempExtractPath = null;
            try
            {
                if (!File.Exists(backupFilePath))
                {
                    return (false, "ملف النسخ الاحتياطي غير موجود!");
                }

                // For ZIP files, verify the zip is valid
                if (Path.GetExtension(backupFilePath).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        using var archive = ZipFile.OpenRead(backupFilePath);
                        if (archive.Entries.Count == 0)
                        {
                            return (false, "الملف المضغوط فارغ!");
                        }
                        
                        // Find the .bak file in the archive
                        var bakEntry = archive.Entries.FirstOrDefault(e => Path.GetExtension(e.Name).Equals(".bak", StringComparison.OrdinalIgnoreCase));
                        if (bakEntry == null)
                        {
                            return (false, "لم يتم العثور على ملف .bak في الملف المضغوط!");
                        }
                        
                        // Extract temporarily to verify
                        tempExtractPath = Path.Combine(Path.GetTempPath(), "ElectricalPOS_Verify_" + Guid.NewGuid());
                        Directory.CreateDirectory(tempExtractPath);
                        bakEntry.ExtractToFile(Path.Combine(tempExtractPath, bakEntry.Name));
                        backupFilePath = Path.Combine(tempExtractPath, bakEntry.Name);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"الملف المضغوط تالف: {ex.Message}");
                    }
                }

                // Verify the backup using T-SQL RESTORE VERIFYONLY
                sqlConnection = new SqlConnection(_connectionString);
                await sqlConnection.OpenAsync();

                var verifyQuery = @"RESTORE VERIFYONLY FROM DISK = @BackupFilePath";
                
                using (var verifyCmd = new SqlCommand(verifyQuery, sqlConnection))
                {
                    verifyCmd.Parameters.AddWithValue("@BackupFilePath", backupFilePath);
                    await verifyCmd.ExecuteNonQueryAsync();
                }

                await sqlConnection.CloseAsync();
                sqlConnection = null;

                // Cleanup temp folder
                if (tempExtractPath != null && Directory.Exists(tempExtractPath))
                {
                    try { Directory.Delete(tempExtractPath, true); } catch { }
                }

                return (true, "ملف النسخ الاحتياطي صالح!");
            }
            catch (SqlException ex)
            {
                return (false, $"ملف النسخ الاحتياطي تالف أو غير صالح: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"فشل التحقق من النسخ الاحتياطي: {ex.Message}");
            }
            finally
            {
                if (sqlConnection != null)
                {
                    if (sqlConnection.State != System.Data.ConnectionState.Closed)
                        await sqlConnection.CloseAsync();
                    sqlConnection.Dispose();
                }
                
                // Cleanup temp folder
                if (tempExtractPath != null && Directory.Exists(tempExtractPath))
                {
                    try { Directory.Delete(tempExtractPath, true); } catch { }
                }
            }
        }

        /// <summary>
        /// Gets the default backup folder path
        /// </summary>
        public string GetDefaultBackupFolder() => _defaultBackupFolder;

        /// <summary>
        /// Creates an automatic scheduled backup
        /// </summary>
        public async Task AutomaticBackupAsync()
        {
            await CreateBackupAsync(
                backupType: "Automatic",
                description: "نسخ احتياطي تلقائي مجدول",
                createdBy: "System",
                compressBackup: true
            );
        }

        #region Private Methods

        private async Task SaveBackupRecordAsync(Models.Backup backup)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                context.Set<Models.Backup>().Add(backup);
                await context.SaveChangesAsync();
            }
            catch
            {
                // If we can't save to database, at least the backup file was created
                // Log this silently
            }
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "بايت", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        #endregion
    }
}
