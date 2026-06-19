using System.Net.Sockets;
using System.Text;

namespace SocketCommon.Services
{
    public static class TextProtocol
    {
        public static void SendText(Socket socket, string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text + "\n");
            socket.Send(data);
        }

        public static string ReceiveText(Socket socket)
        {
            var builder = new StringBuilder();
            byte[] buffer = new byte[1024];

            while (true)
            {
                int bytesRead;
                try
                {
                    bytesRead = socket.Receive(buffer);
                }
                catch
                {
                    return "null";
                }

                if (bytesRead == 0)
                {
                    return "null";
                }

                builder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                if (builder.ToString().Contains("\n"))
                {
                    break;
                }

                if (bytesRead < buffer.Length)
                {
                    break;
                }
            }

            string result = builder.ToString();
            int newlineIndex = result.IndexOf('\n');
            if (newlineIndex >= 0)
            {
                result = result.Substring(0, newlineIndex);
            }

            return result;
        }
    }
}