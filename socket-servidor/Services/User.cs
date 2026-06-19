using System.Net.Sockets;
using Text;
namespace User
{
    public class UserMethods
    {
        public static string RequestUserName(Socket socket)
    {
        TextMethods.SendText(socket, "Nombre:");
        string? userName = TextMethods.ReceiveText(socket)?.Trim();
        if (string.IsNullOrEmpty(userName) || !Validators.Validators.IsValidUserName(userName))
        {
            TextMethods.SendText(socket, "Error. Nombre invalido");
            return string.Empty;
        }

        return userName;
    }
    }
}