using System;
using System.IO;

namespace TitanTrainer.Utils
{
    /// <summary>
    /// Simple file helpers used across the trainer.
    /// </summary>
    public static class FileUtils
    {
        public static string ReadAllTextSafe(string path, string defaultValue = "")
        {
            try
            {
                if (!File.Exists(path)) return defaultValue;
                return File.ReadAllText(path);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}