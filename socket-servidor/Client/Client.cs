
using User;
using Logs;
using Commands;

using System.Net.Sockets;
using SocketCommon.Services;

namespace Client
{
    public class Client
    {
        public static void HandleClient(Socket clientSocket, string LogsDirectory, string UsersDirectory)
        {
            //Solicitar el nombre al cliente
            string userName = UserMethods.RequestUserName(clientSocket);     
            

            //Preparar las carpetas para interactuar con el usuario.
            string userFolder = Path.Combine(UsersDirectory, userName);
            string userFilesFolder = Path.Combine(userFolder, "files");
            Directory.CreateDirectory(userFilesFolder);
            //Cargar los logs
            LogMethods.LogServer($"Cliente {userName} conectado", LogsDirectory);
            LogMethods.LogUser(userName, "Sesion iniciada", UsersDirectory);
            TextProtocol.SendText(clientSocket, "Verificado");//Enviar un check al cliente
            try
            {   //Instruction Decode
                Decode.commandDecode(clientSocket, userName, LogsDirectory, UsersDirectory, userFilesFolder);
            }
            finally
            {   //Guardar los Logs y cerrar el socket
                LogMethods.LogUser(userName, "Sesion finalizada", UsersDirectory);
                LogMethods.LogServer($"Cliente {userName} desconectado", LogsDirectory);
                clientSocket.Close();
            }
        }
    }
}