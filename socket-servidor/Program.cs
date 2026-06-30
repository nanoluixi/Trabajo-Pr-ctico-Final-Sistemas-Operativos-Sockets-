//Socket servidor
using System.Net;
using System.Net.Sockets;


class Program
{
    //Declaración de directorios
    static readonly string BaseDirectory = Directory.GetCurrentDirectory();
    static readonly string LogsDirectory = Path.Combine(BaseDirectory, "Logs");
    static readonly string UsersDirectory = Path.Combine(BaseDirectory, "Users");

    static void Main(string[] args)
    {

        Directory.CreateDirectory(LogsDirectory);
        Directory.CreateDirectory(UsersDirectory);
        //Declaración del socket
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 8080);//En local en el puerto 8080
        serverSocket.Bind(localEndPoint);
        serverSocket.Listen(1);//Poner al socket en modo escucha

        Console.WriteLine("Servidor escuchando en el puerto 8080...");
        //Esperar infinitamente, o hasta que se cierre el servidor que se conecte un cliente
        while (true)
        {
            Socket clientSocket = serverSocket.Accept();
            Console.WriteLine("Cliente conectado: " + clientSocket.RemoteEndPoint?.ToString());

            try
            {
                //Una vez conectado, habilita el cliente
                Client.Client.HandleClient(clientSocket, LogsDirectory, UsersDirectory);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en la sesión del cliente: " + ex.Message);
            }
            finally
            {
                if (clientSocket.Connected)
                {
                    clientSocket.Close();
                }
            }
        }
    }   
}
