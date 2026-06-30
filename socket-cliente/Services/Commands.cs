
using System.Net.Sockets;


using SocketCommon.Services;
namespace Commands
{
    public static class Decode
    {
        public static void commandDecode(Socket clientSocket, string UpDirectory, string DownloadsDirectory, string UppedDirectory)
        {
            while (true)
            {
                Console.WriteLine("Ingrese comandos: listar local, listar servidor, subir <archivo>, descargar <archivo>, borrar <archivo>, bye");
                Console.Write("> ");
                string? input = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(input))
                {
                    continue;
                }

                if (input == "listar local")
                {
                    ListLocalFiles(UpDirectory);
                    continue;
                }

                if (input.StartsWith("subir ", StringComparison.Ordinal))
                {
                    string fileName = input.Substring(6).Trim();
                    if (string.IsNullOrEmpty(fileName))
                    {
                        Console.WriteLine("Comando invalido. Use: subir archivo.extension");
                        continue;
                    }

                    string sourcePath = Path.Combine(UpDirectory, fileName);
                    if (!File.Exists(sourcePath))
                    {
                        Console.WriteLine("Archivo no encontrado en LocalFiles/up: " + fileName);
                        continue;
                    }

                    TextProtocol.SendText(clientSocket, input);
                    string response = TextProtocol.ReceiveText(clientSocket);
                    if (response == null)
                    {
                        Console.WriteLine("Conexión interrumpida por el servidor.");
                        break;
                    }

                    if (response == "Listo")
                    {
                        FileTransferService.SendFile(clientSocket, sourcePath);
                        string uploadResult = TextProtocol.ReceiveText(clientSocket) ?? "Error. No encontrado";
                        Console.WriteLine(uploadResult);
                        if (uploadResult == "Recibido")
                        {
                            MoveUploadedFile(fileName, sourcePath, UppedDirectory);
                        }
                    }
                    else
                    {
                        Console.WriteLine(response);
                    }

                    continue;
                }

                if (input.StartsWith("descargar ", StringComparison.Ordinal))
                {
                    string fileName = input.Substring(10).Trim();
                    if (string.IsNullOrEmpty(fileName))
                    {
                        Console.WriteLine("Comando invalido. Use: descargar archivo.extension");
                        continue;
                    }

                    TextProtocol.SendText(clientSocket, input);
                    string response = TextProtocol.ReceiveText(clientSocket);
                    if (response == null)
                    {
                        Console.WriteLine("Conexión interrumpida por el servidor.");
                        break;
                    }

                    if (response == "Listo")
                    {
                        string destinationPath = Path.Combine(DownloadsDirectory, fileName);
                        bool received = FileTransferService.ReceiveFile(clientSocket, destinationPath);
                        Console.WriteLine(received ? $"Archivo descargado en LocalFiles/downloads/{fileName}" : "Error al recibir el archivo.");
                    }
                    else
                    {
                        Console.WriteLine(response);
                    }

                    continue;
                }

                if (input.StartsWith("borrar ", StringComparison.Ordinal))
                {
                    TextProtocol.SendText(clientSocket, input);
                    string response = TextProtocol.ReceiveText(clientSocket);
                    if (response == null)
                    {
                        Console.WriteLine("Conexión interrumpida por el servidor.");
                        break;
                    }

                    Console.WriteLine(response);
                    continue;
                }

                if (input == "listar servidor")
                {
                    TextProtocol.SendText(clientSocket, input);
                    string response = TextProtocol.ReceiveText(clientSocket);
                    if (response == null)
                    {
                        Console.WriteLine("Conexión interrumpida por el servidor.");
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(response))
                    {
                        Console.WriteLine("No hay archivos en el servidor.");
                    }
                    else
                    {
                        Console.WriteLine("Archivos en servidor:");
                        Console.WriteLine(response);
                    }

                    continue;
                }

                if (input == "bye")
                {
                    TextProtocol.SendText(clientSocket, input);
                    string response = TextProtocol.ReceiveText(clientSocket);
                    if (response != null)
                    {
                        Console.WriteLine(response);
                    }
                    break;
                }

                // Enviar cualquier otro comando al servidor para recibir su validación.
                TextProtocol.SendText(clientSocket, input);
                string fallbackResponse = TextProtocol.ReceiveText(clientSocket);
                if (fallbackResponse == null)
                {
                    Console.WriteLine("Conexión interrumpida por el servidor.");
                    break;
                }

                Console.WriteLine(fallbackResponse);
            }
        }

        //Listar local y moveUp
        static void ListLocalFiles(string UpDirectory)
        {
            string[] files = Directory.GetFiles(UpDirectory);
            if (files.Length == 0)
            {
                Console.WriteLine("No hay archivos en LocalFiles/up.");
                return;
            }

            Console.WriteLine("Archivos en LocalFiles/up:");
            foreach (string file in files)
            {
                Console.WriteLine(Path.GetFileName(file));
            }
        }

        static void MoveUploadedFile(string fileName, string sourcePath, string UppedDirectory)
        {
            string destinationPath = Path.Combine(UppedDirectory, fileName);
            if (File.Exists(destinationPath))
            {
                destinationPath = PathHelper.GetUniqueFilePath(destinationPath);
            }

            File.Move(sourcePath, destinationPath);
            Console.WriteLine($"Archivo movido a LocalFiles/upped/{Path.GetFileName(destinationPath)}");
        }
    }
}