using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace ElectricalPOS.Utilities
{
    /// <summary>
    /// Provides logging and exception handling for database operations.
    /// Useful for diagnosing the real underlying SQL exceptions.
    /// </summary>
    public static class DatabaseLogger
    {
        private static readonly string LogFilePath;
        private static readonly object LockObject = new();

        static DatabaseLogger()
        {
            // Create logs directory in the application base path
            var logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            Directory.CreateDirectory(logsDir);
            LogFilePath = Path.Combine(logsDir, $"ElectricalPOS_Log_{DateTime.Now:yyyyMMdd}.log");
        }

        /// <summary>
        /// Logs an informational message
        /// </summary>
        public static void LogInfo(string message, 
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            Log("INFO", message, null, memberName, filePath, lineNumber);
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        public static void LogWarning(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            Log("WARN", message, null, memberName, filePath, lineNumber);
        }

        /// <summary>
        /// Logs an error with full exception details
        /// </summary>
        public static void LogError(string message, Exception? exception,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            Log("ERROR", message, exception, memberName, filePath, lineNumber);
        }

        /// <summary>
        /// Logs a database operation with timing
        /// </summary>
        public static void LogDatabaseOperation(string operation, TimeSpan duration, bool success,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            var status = success ? "SUCCESS" : "FAILED";
            var message = $"Database {operation} - {status} (Duration: {duration.TotalMilliseconds:F2}ms)";
            Log(status, message, null, memberName, filePath, lineNumber);
        }

        private static void Log(string level, string message, Exception? exception,
            string memberName, string filePath, int lineNumber)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var fileName = Path.GetFileName(filePath);
            var logEntry = $"[{timestamp}] [{level}] [{fileName}:{lineNumber}] [{memberName}] {message}";

            if (exception != null)
            {
                logEntry += $"\n  Exception: {exception.GetType().FullName}";
                logEntry += $"\n  Message: {exception.Message}";
                logEntry += $"\n  StackTrace: {exception.StackTrace}";

                // Include inner exception for RetryLimitExceededException
                if (exception.InnerException != null)
                {
                    logEntry += $"\n  InnerException: {exception.InnerException.GetType().FullName}";
                    logEntry += $"\n  InnerMessage: {exception.InnerException.Message}";
                    logEntry += $"\n  InnerStackTrace: {exception.InnerException.StackTrace}";
                }
            }

            // Also write to debug output
            System.Diagnostics.Debug.WriteLine(logEntry);

            // Write to file (thread-safe)
            lock (LockObject)
            {
                try
                {
                    File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
                }
                catch
                {
                    // Ignore file write errors
                }
            }
        }

        /// <summary>
        /// Extracts the real underlying SQL exception from RetryLimitExceededException
        /// Useful for debugging - temporarily use this to see the actual SQL error
        /// </summary>
        public static Exception? GetUnderlyingException(Exception ex)
        {
            // Handle RetryLimitExceededException
            if (ex.GetType().Name == "RetryLimitExceededException")
            {
                // The inner exception contains the actual SQL error
                if (ex.InnerException != null)
                {
                    LogError("Underlying SQL exception extracted:", ex.InnerException);
                    return ex.InnerException;
                }
            }

            // Handle aggregate exceptions (multiple exceptions)
            if (ex is AggregateException aggregateException)
            {
                foreach (var innerEx in aggregateException.InnerExceptions)
                {
                    if (innerEx.GetType().Name == "RetryLimitExceededException" && innerEx.InnerException != null)
                    {
                        LogError("Underlying SQL exception from AggregateException:", innerEx.InnerException);
                        return innerEx.InnerException;
                    }
                }
            }

            return ex;
        }

        /// <summary>
        /// Gets the path to the log file
        /// </summary>
        public static string GetLogFilePath() => LogFilePath;
    }
}
