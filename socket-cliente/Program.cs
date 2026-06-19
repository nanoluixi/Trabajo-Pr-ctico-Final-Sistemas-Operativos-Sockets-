//Socket cliente

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SocketCommon.Services;

class Program
{
    static readonly string BaseDirectory = Directory.GetCurrentDirectory();
    static readonly string LocalFilesDirectory = Path.Combine(BaseDirectory, "LocalFiles");
    static readonly string UpDirectory = Path.Combine(LocalFilesDirectory, "up");
    static readonly string UppedDirectory = Path.Combine(LocalFilesDirectory, "upped");
    static readonly string DownloadsDirectory = Path.Combine(LocalFilesDirectory, "downloads");

    static void Main(string[] args)
    {
        Directory.CreateDirectory(UpDirectory);
        Directory.CreateDirectory(UppedDirectory);
        Directory.CreateDirectory(DownloadsDirectory);

        Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8080);
            clientSocket.Connect(serverEndPoint);
            Console.WriteLine("Conectado al servidor: " + serverEndPoint.ToString());

            string serverPrompt = TextProtocol.ReceiveText(clientSocket);
            if (serverPrompt == null)
            {
                Console.WriteLine("No se recibió prompt del servidor. Conexión cerrada.");
                return;
            }

            Console.WriteLine(serverPrompt);
            //Console.Write("Nombre: ");
            string userName = Console.ReadLine()?.Trim() ?? string.Empty;
            TextProtocol.SendText(clientSocket, userName);

            string serverResponse = TextProtocol.ReceiveText(clientSocket);
            if (serverResponse == null)
            {
                Console.WriteLine("Servidor cerró la conexión.");
                return;
            }

            if (serverResponse.StartsWith("Error. Nombre invalido", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(serverResponse);
                return;
            }
            if(serverResponse == "Verfificado")
            {
                Console.WriteLine("Usuario verificado por el servidor.");
            }
            Console.WriteLine("Sesión iniciada como: " + userName);

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
                    ListLocalFiles();
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
                            MoveUploadedFile(fileName, sourcePath);
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
        catch (Exception ex)
        {
            Console.WriteLine("Error al conectar o comunicarse con el servidor: " + ex.Message);
        }
        finally
        {
            if (clientSocket.Connected)
            {
                clientSocket.Close();
            }

            Console.WriteLine("Conexión cerrada.");
        }
    }

    static void ListLocalFiles()
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

    static void MoveUploadedFile(string fileName, string sourcePath)
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
    
