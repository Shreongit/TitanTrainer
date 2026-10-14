using System;
using System.IO;
using System.Text;
using System.Threading;

namespace TitanTrainer.Core
{
    /// <summary>
    /// Thread-safe logger for Titan Trainer.
    /// Writes timestamped entries to a log file and optionally to the game notification system.
    /// </summary>
    public static class Logger
    {
        private static readonly object Lock = new object();
        private static string _logDirectory = "Logs";
        private static string _componentName = "TitanTrainer";
        private static string _currentLogPath;

        /// <summary>
        /// Initializes the logger for the given component. Creates a per-day log file under Logs/{component}/.
        /// </summary>
        /// <param name="componentName">Name of the component (e.g., TitanTrainer).</param>
        public static void Initialize(string componentName)
        {
            if (string.IsNullOrWhiteSpace(componentName))
            {
                componentName = "TitanTrainer";
            }

            _componentName = componentName;
            try
            {
                _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", _componentName);
                Directory.CreateDirectory(_logDirectory);
                _currentLogPath = Path.Combine(_logDirectory, $"{DateTime.UtcNow:yyyy-MM-dd}.log");
                WriteEntry("Logger initialized.", "Info");
            }
            catch
            {
                // If logging initialization fails, swallow exceptions to not break the script host.
            }
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The informational message.</param>
        public static void Info(string message) => WriteEntry(message, "INFO");

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The warning message.</param>
        public static void Warn(string message) => WriteEntry(message, "WARN");

        /// <summary>
        /// Logs an error message and optional exception.
        /// </summary>
        /// <param name="message">The error message or exception text.</param>
        public static void Error(string message) => WriteEntry(message, "ERROR");

        /// <summary>
        /// Logs a debug-level message. Use sparingly in production.
        /// </summary>
        /// <param name="message">The debug message.</param>
        public static void Debug(string message) => WriteEntry(message, "DEBUG");

        private static void WriteEntry(string message, string level)
        {
            try
            {
                lock (Lock)
                {
                    if (string.IsNullOrEmpty(_currentLogPath))
                    {
                        Initialize(_componentName);
                    }

                    var timestamp = DateTime.UtcNow.ToString("o"); // ISO 8601
                    var line = $"{timestamp} [{level}] {message}{Environment.NewLine}";
                    File.AppendAllText(_currentLogPath, line, Encoding.UTF8);
                }
            }
            catch
            {
                // Swallow exceptions to avoid cascading failures in the game environment.
            }
        }
    }
}