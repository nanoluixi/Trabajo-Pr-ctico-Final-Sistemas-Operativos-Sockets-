using Text;
using User;
using Logs;
using Services;
using System.Net.Sockets;
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
        TextMethods.SendText(clientSocket, "Verfificado");
        try
        {
            while (true)
            {
                string command = TextMethods.ReceiveText(clientSocket);
                if (command == null)
                {
                    Console.WriteLine("Cliente desconectado de forma inesperada.");
                    break;
                }

                command = command.Trim();
                if (command.Length == 0)
                {
                    TextMethods.SendText(clientSocket, "Error. Comando invalido");
                    continue;
                }

                LogMethods.LogServer($"{userName} -> {command}", LogsDirectory);
                LogMethods.LogUser(userName, command, UsersDirectory);

                if (command == "bye")
                {
                    TextMethods.SendText(clientSocket, "Bye");
                    break;
                }

                if (command == "listar servidor")
                {
                    string[] files = Directory.GetFiles(userFilesFolder);
                    if (files.Length == 0)
                    {
                        TextMethods.SendText(clientSocket, string.Empty);
                    }
                    else
                    {
                        string result = string.Join("\n", Array.ConvertAll(files, Path.GetFileName));
                        TextMethods.SendText(clientSocket, result);
                    }
                    continue;
                }

                if (command.StartsWith("subir ", StringComparison.Ordinal))
                {
                    string fileName = command.Substring(6).Trim();
                    if (!Validators.Validators.IsValidFileName(fileName))
                    {
                        TextMethods.SendText(clientSocket, "Error. Comando invalido");
                        continue;
                    }

                    string destinationPath = FileTransferService.GetUniqueFilePath(Path.Combine(userFilesFolder, fileName));
                    TextMethods.SendText(clientSocket, "Listo");

                    bool received = FileTransferService.RecibirArchivo(clientSocket, destinationPath);
                    if (received)
                    {
                        TextMethods.SendText(clientSocket, "Recibido");
                    }
                    else
                    {
                        TextMethods.SendText(clientSocket, "Error. No encontrado");
                    }
                    continue;
                }

                if (command.StartsWith("descargar ", StringComparison.Ordinal))
                {
                    string fileName = command.Substring(10).Trim();
                    if (!Validators.Validators.IsValidFileName(fileName))
                    {
                        TextMethods.SendText(clientSocket, "Error. Comando invalido");
                        continue;
                    }

                    string sourcePath = Path.Combine(userFilesFolder, fileName);
                    if (!File.Exists(sourcePath))
                    {
                        TextMethods.SendText(clientSocket, "Error. No encontrado");
                        continue;
                    }

                    TextMethods.SendText(clientSocket, "Listo");
                    FileTransferService.EnviarArchivo(clientSocket, sourcePath);
                    continue;
                }

                if (command.StartsWith("borrar ", StringComparison.Ordinal))
                {
                    string fileName = command.Substring(7).Trim();
                    if (!Validators.Validators.IsValidFileName(fileName))
                    {
                        TextMethods.SendText(clientSocket, "Error. Comando invalido");
                        continue;
                    }

                    string filePath = Path.Combine(userFilesFolder, fileName);
                    if (!File.Exists(filePath))
                    {
                        TextMethods.SendText(clientSocket, "Error. No encontrado");
                        continue;
                    }

                    File.Delete(filePath);
                    TextMethods.SendText(clientSocket, "Recibido");
                    continue;
                }

                TextMethods.SendText(clientSocket, "Error. Comando invalido");
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