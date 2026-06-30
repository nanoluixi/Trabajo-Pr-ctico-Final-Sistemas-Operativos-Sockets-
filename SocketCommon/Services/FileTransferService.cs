using System.Net.Sockets;

namespace SocketCommon.Services
{
    public static class FileTransferService
    {
        //Servicios de transferencia de archivos comunes entre cliente-servidor
        private const int BufferSize = 8192;

        public static void SendFile(Socket socket, string sourcePath)
        {
            FileInfo fileInfo = new FileInfo(sourcePath);
            TextProtocol.SendText(socket, fileInfo.Length.ToString());

            using FileStream fileStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[BufferSize];
            int bytesRead;

            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                socket.Send(buffer, bytesRead, SocketFlags.None);
            }
        }

        public static bool ReceiveFile(Socket socket, string destinationPath)
        {
            string sizeText = TextProtocol.ReceiveText(socket);
            if (string.IsNullOrEmpty(sizeText) || !long.TryParse(sizeText.Trim(), out long fileSize) || fileSize < 0)
            {
                return false;
            }

            using FileStream fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
            long remaining = fileSize;
            byte[] buffer = new byte[BufferSize];

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
    }
}