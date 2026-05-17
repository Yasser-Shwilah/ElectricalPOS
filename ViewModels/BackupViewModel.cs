using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectricalPOS.Models;
using ElectricalPOS.Services;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace ElectricalPOS.ViewModels
{
    public partial class BackupViewModel : ObservableObject
    {
        private readonly BackupService _backupService;

        [ObservableProperty]
        private ObservableCollection<Backup> _backups = new();

        [ObservableProperty]
        private Backup? _selectedBackup;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _isBackupInProgress;

        [ObservableProperty]
        private bool _isRestoreInProgress;

        [ObservableProperty]
        private int _progressValue;

        [ObservableProperty]
        private string _progressText = string.Empty;

        // Statistics
        [ObservableProperty]
        private int _totalBackups;

        [ObservableProperty]
        private string _totalSize = "0 بايت";

        [ObservableProperty]
        private string _lastBackupDate = "لم يتم";

        // Settings
        [ObservableProperty]
        private string _backupFolder = string.Empty;

        [ObservableProperty]
        private bool _compressBackup = true;

        [ObservableProperty]
        private int _retentionDays = 30;

        public BackupViewModel()
        {
            _backupService = new BackupService();
            BackupFolder = _backupService.GetDefaultBackupFolder();
            _ = LoadBackupsAsync();
        }

        [RelayCommand]
        private async Task LoadBackupsAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "جاري تحميل السجلات...";

                var backups = await _backupService.GetAllBackupsAsync();
                Backups.Clear();
                foreach (var backup in backups)
                {
                    Backups.Add(backup);
                }

                // Update statistics
                await LoadStatisticsAsync();

                StatusMessage = $"تم تحميل {backups.Count} سجل";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في التحميل: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadStatisticsAsync()
        {
            var (totalBackups, totalSize, lastBackup) = await _backupService.GetBackupStatisticsAsync();
            TotalBackups = totalBackups;
            TotalSize = FormatFileSize(totalSize);
            LastBackupDate = lastBackup?.ToString("yyyy/MM/dd HH:mm") ?? "لم يتم";
        }

        [RelayCommand]
        private async Task CreateBackupAsync()
        {
            try
            {
                IsBackupInProgress = true;
                ProgressValue = 0;
                ProgressText = "جاري إنشاء النسخ الاحتياطي...";
                StatusMessage = "جاري إنشاء النسخ الاحتياطي...";

                ProgressValue = 20;

                var (success, message, filePath) = await _backupService.CreateBackupAsync(
                    customPath: BackupFolder,
                    backupType: "Manual",
                    description: "نسخ احتياطي يدوي",
                    createdBy: "Admin",
                    compressBackup: CompressBackup
                );

                ProgressValue = 100;

                if (success)
                {
                    StatusMessage = message;
                    MessageBox.Show(message, "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadBackupsAsync();
                }
                else
                {
                    StatusMessage = message;
                    MessageBox.Show(message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
                MessageBox.Show($"فشل إنشاء النسخ الاحتياطي: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBackupInProgress = false;
                ProgressValue = 0;
                ProgressText = string.Empty;
            }
        }

        [RelayCommand]
        private async Task RestoreBackupAsync()
        {
            if (SelectedBackup == null)
            {
                MessageBox.Show("الرجاء اختيار نسخة احتياطية للاستعادة!", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"هل أنت متأكد من استعادة قاعدة البيانات من النسخة الاحتياطية؟\n\nملاحظة: سيتم استبدال جميع البيانات الحالية!",
                "تأكيد الاستعادة",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                IsRestoreInProgress = true;
                ProgressText = "جاري استعادة قاعدة البيانات...";
                StatusMessage = "جاري استعادة قاعدة البيانات...";

                var (success, message) = await _backupService.RestoreBackupAsync(SelectedBackup.FilePath);

                if (success)
                {
                    StatusMessage = message;
                    MessageBox.Show(message, "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    StatusMessage = message;
                    MessageBox.Show(message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
                MessageBox.Show($"فشل استعادة قاعدة البيانات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsRestoreInProgress = false;
                ProgressText = string.Empty;
            }
        }

        [RelayCommand]
        private async Task RestoreFromFileAsync()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "اختر ملف النسخ الاحتياطي",
                Filter = "ملفات النسخ الاحتياطي (*.bak;*.zip)|*.bak;*.zip|كل الملفات (*.*)|*.*",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var result = MessageBox.Show(
                    $"هل أنت متأكد من استعادة قاعدة البيانات من:\n{openFileDialog.FileName}\n\nملاحظة: سيتم استبدال جميع البيانات الحالية!",
                    "تأكيد الاستعادة",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;

                try
                {
                    IsRestoreInProgress = true;
                    ProgressText = "جاري استعادة قاعدة البيانات...";
                    StatusMessage = "جاري استعادة قاعدة البيانات...";

                    var (success, message) = await _backupService.RestoreBackupAsync(openFileDialog.FileName);

                    if (success)
                    {
                        StatusMessage = message;
                        MessageBox.Show(message, "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        StatusMessage = message;
                        MessageBox.Show(message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    StatusMessage = $"خطأ: {ex.Message}";
                    MessageBox.Show($"فشل استعادة قاعدة البيانات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsRestoreInProgress = false;
                    ProgressText = string.Empty;
                }
            }
        }

        [RelayCommand]
        private async Task DeleteBackupAsync()
        {
            if (SelectedBackup == null)
            {
                MessageBox.Show("الرجاء اختيار نسخة احتياطية للحذف!", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"هل أنت متأكد من حذف النسخة الاحتياطية:\n{SelectedBackup.FileName}\n\nسيتم حذف الملف أيضاً!",
                "تأكيد الحذف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                IsLoading = true;
                var (success, message) = await _backupService.DeleteBackupAsync(SelectedBackup.Id, true);

                if (success)
                {
                    StatusMessage = message;
                    MessageBox.Show(message, "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadBackupsAsync();
                }
                else
                {
                    StatusMessage = message;
                    MessageBox.Show(message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
                MessageBox.Show($"فشل حذف النسخة الاحتياطية: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task CleanupOldBackupsAsync()
        {
            var result = MessageBox.Show(
                $"هل أنت متأكد من حذف النسخ الاحتياطية الأقدم من {RetentionDays} يوم؟",
                "تأكيد التنظيف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                IsLoading = true;
                var (deletedCount, message) = await _backupService.CleanupOldBackupsAsync(RetentionDays);
                StatusMessage = message;
                MessageBox.Show(message, "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadBackupsAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
                MessageBox.Show($"فشل تنظيف النسخ الاحتياطية: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task VerifyBackupAsync()
        {
            if (SelectedBackup == null)
            {
                MessageBox.Show("الرجاء اختيار نسخة احتياطية للتحقق!", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "جاري التحقق من النسخة الاحتياطية...";

                var (isValid, message) = await _backupService.VerifyBackupAsync(SelectedBackup.FilePath);

                StatusMessage = message;
                MessageBox.Show(message, isValid ? "نجاح" : "خطأ", MessageBoxButton.OK, 
                    isValid ? MessageBoxImage.Information : MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
                MessageBox.Show($"فشل التحقق: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void BrowseBackupFolder()
        {
            var folderDialog = new OpenFolderDialog
            {
                Title = "اختر مجلد النسخ الاحتياطي"
            };

            if (folderDialog.ShowDialog() == true)
            {
                BackupFolder = folderDialog.FolderName;
            }
        }

        [RelayCommand]
        private void OpenBackupFolder()
        {
            if (Directory.Exists(BackupFolder))
            {
                System.Diagnostics.Process.Start("explorer.exe", BackupFolder);
            }
            else
            {
                MessageBox.Show("المجلد غير موجود!", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadBackupsAsync();
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
    }
}
