using System;
using System.IO;

namespace Pinnit
{
    public static class Logger
    {
        private static string LogPath = "debug.log";

        public static void Log(string message)
        {
            try
            {
                File.AppendAllText(LogPath, $"{DateTime.Now}: {message}{Environment.NewLine}");
            }
            catch { }
        }
    }
}
