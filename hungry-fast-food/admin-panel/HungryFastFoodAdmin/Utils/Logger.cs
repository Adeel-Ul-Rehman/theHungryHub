// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Utils\Logger.cs

using System;
using System.IO;

namespace HungryFastFoodAdmin
{
    public static class Logger
    {
        private static readonly string _logDirectory;

        static Logger()
        {
            _logDirectory = ConfigManager.GetAppSetting("LogPath", "D:\\HungryFastFood\\logs\\");
            try
            {
                if (!Directory.Exists(_logDirectory))
                {
                    Directory.CreateDirectory(_logDirectory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create log directory: {ex.Message}");
            }
        }

        public static void Log(string message, string level = "INFO")
        {
            try
            {
                string logFile = Path.Combine(_logDirectory, $"{DateTime.Now:yyyy-MM-dd}.log");
                string logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level.ToUpper()}] {message}";

                // Console output for debugging
                Console.WriteLine(logLine);

                // Write to log file
                lock (_logDirectory)
                {
                    File.AppendAllText(logFile, logLine + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logging failed: {ex.Message}");
            }
        }

        public static void LogError(string message, Exception ex)
        {
            if (ex != null)
            {
                Log($"{message} | Exception: {ex.Message} | StackTrace: {ex.StackTrace}", "ERROR");
            }
            else
            {
                Log(message, "ERROR");
            }
        }
    }
}
