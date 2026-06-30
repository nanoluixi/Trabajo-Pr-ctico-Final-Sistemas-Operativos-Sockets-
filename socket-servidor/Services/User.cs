using System.Net.Sockets;
using SocketCommon.Services;
namespace User
{
    public class UserMethods
    {
        public static string RequestUserName(Socket socket)
        {
            //Mëtodo para solicitar el nombre de usuario al usuario que se conectó al socket.
            TextProtocol.SendText(socket, "Nombre:");
            string? userName = TextProtocol.ReceiveText(socket)?.Trim();
            if (string.IsNullOrEmpty(userName) || !Validators.Validators.IsValidUserName(userName))
            {
                TextProtocol.SendText(socket, "Error. Nombre invalido");
                return string.Empty;
            }

            return userName;
        }
    }
}