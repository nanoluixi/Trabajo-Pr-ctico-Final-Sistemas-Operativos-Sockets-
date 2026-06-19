using System.IO;
using System.Net.Sockets;
using Text;

namespace Services
{
    public class FileTransferService
    {
        public static void EnviarArchivo(Socket socket, string sourcePath)
        {
            FileInfo fileInfo = new FileInfo(sourcePath);
            TextMethods.SendText(socket, fileInfo.Length.ToString());

            using FileStream fileStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[8192];
            int bytesRead;

            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                socket.Send(buffer, bytesRead, SocketFlags.None);
            }
        }
        public static bool RecibirArchivo(Socket socket, string destinationPath)
    {
        string sizeText = TextMethods.ReceiveText(socket);
        if (string.IsNullOrEmpty(sizeText) || !long.TryParse(sizeText.Trim(), out long fileSize) || fileSize < 0)
        {
            return false;
        }

        using FileStream fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
        long remaining = fileSize;
        byte[] buffer = new byte[8192];

        while (remaining > 0)
        {
            int toRead = remaining > buffer.Length ? buffer.Length : (int)remaining;
            int bytesRead = socket.Receive(buffer, 0, toRead, SocketFlags.None);
            if (bytesRead <= 0)
            {
                return false;
            }

            fileStream.Write(buffer, 0, bytesRead);
            remaining -= bytesRead;
        }

        return true;
    }
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
