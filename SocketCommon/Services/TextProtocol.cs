using System.Net.Sockets;
using System.Text;

namespace SocketCommon.Services
{
    public static class TextProtocol
    //Protocolos para el envio y recibo de mensajes entre sockets
    {
        public static void SendText(Socket socket, string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text + "\n");
            socket.Send(data);
        }

        public static string? ReceiveText(Socket socket)
        {
            var builder = new StringBuilder();

            while (true)
            {
                byte[] oneByte = new byte[1];
                int bytesRead;
                try
                {
                    bytesRead = socket.Receive(oneByte, 0, 1, SocketFlags.None);
                }
                catch
                {
                    return null;
                }

                if (bytesRead == 0)
                {
                    return null;
                }

                char c = (char)oneByte[0];
                if (c == '\n')
                {
                    break;
                }

                builder.Append(c);
            }

            return builder.ToString();
        }
    }
}