
using System.Net.Sockets;


using SocketCommon.Services;
namespace Commands
{
    public static class Decode
    {
        public static void commandDecode(Socket clientSocket, string UpDirectory, string DownloadsDirectory, string UppedDirectory)
        {
            string[] comandosDisponibles = [
                "Comandos",
                "bye: Para salir del servidor.",
                "listar servidor: Para listar los archivos del servidor.",
                "listar local: Para listar los archivos locales.",
                "subir [nombre_de_archivo.extensión]: Para subir un archivo de la carpeta up al servidor.",
                "descargar [nombre_de_archivo.extensión]: Para descargar un archivo que esté subido en el servidor.",
                "borrar [nombre_de_archivo.extensión]: Para borrar un archivo que esté subido al servidor.",
                "help: Para ver los comandos disponibles."];
            while (true)
            {
                Console.Write("> ");
                string? input = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(input))
                {
                    continue;
                }

                if (input == "listar local")
                {
                    ListLocalFiles(UpDirectory, "LocalFiles/up.");
                    continue;
                }
                if (input == "listar descargas")
                {
                    ListLocalFiles(DownloadsDirectory, "LocalFiles/dowloads");
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

                    if (response == "Servidor: Listo")
                    {
                        FileTransferService.SendFile(clientSocket, sourcePath);
                        string uploadResult = TextProtocol.ReceiveText(clientSocket) ?? "Error. No encontrado";
                        Console.WriteLine(uploadResult);
                        if (uploadResult == "Servidor: Recibido")
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

                    if (response == "Servidor: Listo")
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
                if(input == "help")
                {
                    TextProtocol.SendText(clientSocket, "help");
                    foreach (var comando in comandosDisponibles)
                    {
                        Console.WriteLine(comando);
                    }
                    continue;
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
        static void ListLocalFiles(string UpDirectory, string nombreDirectorio)
        {
            string[] files = Directory.GetFiles(UpDirectory);
            if (files.Length == 0)
            {
                Console.WriteLine("No hay archivos en "+ nombreDirectorio);
                return;
            }

            Console.WriteLine("Archivos en "+nombreDirectorio+":");
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