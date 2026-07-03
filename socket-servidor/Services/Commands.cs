
using System.Net.Sockets;
using Logs;

using SocketCommon.Services;
namespace Commands
{
    public static class Decode
    {
        public static void commandDecode(Socket clientSocket, string userName, string LogsDirectory, string UsersDirectory, string userFilesFolder)
        {
            /*Comandos disponibles:
                    bye: Salir
                    listar servidor: Lista los archivos del servidor
                    subir: Sube archivos al servidor (recibe archivos)
                    descargar: Descarga archivos del servidor (enviar archivos)
                    borrar: Borra un archivo del servidor
                    help: Envia al usuario los comandos disponibles
                */
            while (true)
            {
                string command = TextProtocol.ReceiveText(clientSocket);
                //DecodeCommand(command)
                if (command == null)
                {
                    Console.WriteLine("Servidor: "+"Cliente desconectado de forma inesperada.");
                    break;
                }

                command = command.Trim();
                if (command.Length == 0)
                {
                    TextProtocol.SendText(clientSocket,"Servidor: "+ "Error. Comando invalido");
                    continue;
                }

                LogMethods.LogServer($"{userName} -> {command}", LogsDirectory);
                LogMethods.LogUser(userName, command, UsersDirectory);

                if (command == "bye")
                {
                    TextProtocol.SendText(clientSocket,"Servidor: "+ "Bye");
                    break;
                }

                if (command == "listar servidor")
                {
                    string[] files = Directory.GetFiles(userFilesFolder);
                    if (files.Length == 0)
                    {
                        TextProtocol.SendText(clientSocket, "Servidor: No hay archivos en el servidor.");
                    }
                    else
                    {
                        string result = string.Join("\n", Array.ConvertAll(files, Path.GetFileName));
                        TextProtocol.SendText(clientSocket, "Servidor: "+result);
                    }
                    continue;
                }

                if (command.StartsWith("subir ", StringComparison.Ordinal))
                {
                    string fileName = command.Substring(6).Trim();
                    if (!Validators.Validators.IsValidFileName(fileName))
                    {
                        TextProtocol.SendText(clientSocket, "Servidor: "+"Error. Comando invalido");
                        continue;
                    }

                    string destinationPath = PathHelper.GetUniqueFilePath(Path.Combine(userFilesFolder, fileName));
                    TextProtocol.SendText(clientSocket, "Servidor: Listo");

                    bool received = FileTransferService.ReceiveFile(clientSocket, destinationPath);
                    if (received)
                    {
                        TextProtocol.SendText(clientSocket, "Servidor: Recibido");
                    }
                    else
                    {
                        TextProtocol.SendText(clientSocket, "Servidor: "+"Error. No encontrado");
                    }
                    continue;
                }

                if (command.StartsWith("descargar ", StringComparison.Ordinal))
                {
                    string fileName = command.Substring(10).Trim();
                    if (!Validators.Validators.IsValidFileName(fileName))
                    {
                        TextProtocol.SendText(clientSocket, "Servidor: "+"Error. Comando invalido");
                        continue;
                    }

                    string sourcePath = Path.Combine(userFilesFolder, fileName);
                    if (!File.Exists(sourcePath))
                    {
                        TextProtocol.SendText(clientSocket, "Servidor: "+"Error. No encontrado");
                        continue;
                    }

                    TextProtocol.SendText(clientSocket, "Servidor: Listo");
                    FileTransferService.SendFile(clientSocket, sourcePath);
                    continue;
                }

                if (command.StartsWith("borrar ", StringComparison.Ordinal))
                {
                    string fileName = command.Substring(7).Trim();
                    if (!Validators.Validators.IsValidFileName(fileName))
                    {
                        TextProtocol.SendText(clientSocket, "Servidor: "+"Error. Comando invalido");
                        continue;
                    }

                    string filePath = Path.Combine(userFilesFolder, fileName);
                    if (!File.Exists(filePath))
                    {
                        TextProtocol.SendText(clientSocket, "Servidor: "+"Error. No encontrado");
                        continue;
                    }

                    File.Delete(filePath);
                    TextProtocol.SendText(clientSocket, "Servidor: Recibido");
                    continue;
                }
                // Si no hay match informar al usuario
                TextProtocol.SendText(clientSocket, "Servidor: "+"Error. Comando invalido, envie 'help' para conocer los comandos disponibles.");
            }
        }
    }
}