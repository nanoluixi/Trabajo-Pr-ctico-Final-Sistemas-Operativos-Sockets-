namespace SocketCommon.Services
{
    public static class PathHelper
    {
        public static string GetUniqueFilePath(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return filePath;
            }

            string directory = Path.GetDirectoryName(filePath) ?? string.Empty;
            string name = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);

            int counter = 1;
            string candidate;
            do
            {
                candidate = Path.Combine(directory, $"{name}({counter}){extension}");
                counter++;
            }
            while (File.Exists(candidate));

            return candidate;
        }
    }
}