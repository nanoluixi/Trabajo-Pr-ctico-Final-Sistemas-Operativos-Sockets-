//Socket cliente

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SocketCommon.Services;
using Commands;

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
            if(serverResponse == "Verificado")
            {
                Console.WriteLine("Usuario verificado por el servidor.");
            }
            Console.WriteLine("Sesión iniciada como: " + userName);

            //CommandDecode
            Decode.commandDecode(clientSocket, UpDirectory, DownloadsDirectory, UppedDirectory);
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
}
    
