
using User;
using Logs;

using System.Net.Sockets;
using SocketCommon.Services;

namespace Client
{
    public class Client
    {
        public static void HandleClient(Socket clientSocket, string LogsDirectory, string UsersDirectory)
    {
        string userName = UserMethods.RequestUserName(clientSocket);
        if (string.IsNullOrEmpty(userName))
        {
            return;
        }

        string userFolder = Path.Combine(UsersDirectory, userName);
        string userFilesFolder = Path.Combine(userFolder, "files");
        Directory.CreateDirectory(userFilesFolder);

        LogMethods.LogServer($"Cliente {userName} conectado", LogsDirectory);
        LogMethods.LogUser(userName, "Sesion iniciada", UsersDirectory);
        TextProtocol.SendText(clientSocket, "Verfificado");
        try
        {
            while (true)
            {
                string command = TextProtocol.ReceiveText(clientSocket);
                if (command == null)
                {
                    Console.WriteLine("Cliente desconectado de forma inesperada.");
                    break;
                }

                command = command.Trim();
                if (command.Length == 0)
                {
                    TextProtocol.SendText(clientSocket, "Error. Comando invalido");
                    continue;
                }

                LogMethods.LogServer($"{userName} -> {command}", LogsDirectory);
                LogMethods.LogUser(userName, command, UsersDirectory);

                if (command == "bye")
                {
                    TextProtocol.SendText(clientSocket, "Bye");
                    break;
                }

                if (command == "listar servidor")
                {
                    string[] files = Directory.GetFiles(userFilesFolder);
                    if (files.Length == 0)
                    {
                        TextProtocol.SendText(clientSocket, string.Empty);
                    }
                    else
                    {
                        string result = string.Join("\n", Array.ConvertAll(files, Path.GetFileName));
                        TextProtocol.SendText(clientSocket, result);
                    }
                    continue;
                }

                if (command.StartsWith("subir ", StringComparison.Ordinal))
                {
                    string fileName = command.Substring(6).Trim();
                    if (!Validators.Validators.IsValidFileName(fileName))
                    {
                        TextProtocol.SendText(clientSocket, "Error. Comando invalido");
                        continue;
                    }

                    string destinationPath = PathHelper.GetUniqueFilePath(Path.Combine(userFilesFolder, fileName));
                    TextProtocol.SendText(clientSocket, "Listo");

                    bool received = SocketCommon.Services.FileTransferService.ReceiveFile(clientSocket, destinationPath);
                    if (received)
                    {
                        TextProtocol.SendText(clientSocket, "Recibido");
                    }
                    else
                    {
                        TextProtocol.SendText(clientSocket, "Error. No encontrado");
                    }
                    continue;
                }

                if (command.StartsWith("descargar ", StringComparison.Ordinal))
                {
                    string fileName = command.Substring(10).Trim();
                    if (!Validators.Validators.IsValidFileName(fileName))
                    {
                        TextProtocol.SendText(clientSocket, "Error. Comando invalido");
                        continue;
                    }

                    string sourcePath = Path.Combine(userFilesFolder, fileName);
                    if (!File.Exists(sourcePath))
                    {
                        TextProtocol.SendText(clientSocket, "Error. No encontrado");
                        continue;
                    }

                    TextProtocol.SendText(clientSocket, "Listo");
                    SocketCommon.Services.FileTransferService.SendFile(clientSocket, sourcePath);
                    continue;
                }

                if (command.StartsWith("borrar ", StringComparison.Ordinal))
                {
                    string fileName = command.Substring(7).Trim();
                    if (!Validators.Validators.IsValidFileName(fileName))
                    {
                        TextProtocol.SendText(clientSocket, "Error. Comando invalido");
                        continue;
                    }

                    string filePath = Path.Combine(userFilesFolder, fileName);
                    if (!File.Exists(filePath))
                    {
                        TextProtocol.SendText(clientSocket, "Error. No encontrado");
                        continue;
                    }

                    File.Delete(filePath);
                    TextProtocol.SendText(clientSocket, "Recibido");
                    continue;
                }

                TextProtocol.SendText(clientSocket, "Error. Comando invalido");
            }
        }
        finally
        {
            LogMethods.LogUser(userName, "Sesion finalizada", UsersDirectory);
            LogMethods.LogServer($"Cliente {userName} desconectado", LogsDirectory);
            clientSocket.Close();
        }
    }
    }
}