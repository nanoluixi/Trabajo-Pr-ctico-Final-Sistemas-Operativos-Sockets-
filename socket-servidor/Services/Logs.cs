namespace Logs
{
    public static class LogMethods
    {
        public static void LogServer(string message, string LogsDirectory)
        {   
            //Guarda los logs a un txt del servidor.
            string logLine = FormatLogLine(message);
            string serverLogPath = Path.Combine(LogsDirectory, "server.log");
            File.AppendAllText(serverLogPath, logLine + Environment.NewLine);
            Console.WriteLine(logLine);
        }

        public static void LogUser(string userName, string message, string UsersDirectory)
        {   
            //Guarda los logs en un archivo txt dentro de cada usuario.
            string logLine = FormatLogLine(message);
            string userLogPath = Path.Combine(UsersDirectory, userName, "log.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(userLogPath) ?? string.Empty);
            File.AppendAllText(userLogPath, logLine + Environment.NewLine);
        }

        static string FormatLogLine(string message)
        {
            return $"{DateTime.Now:yyyy-MM-dd HH:mm} {message}";
        }
    }
}