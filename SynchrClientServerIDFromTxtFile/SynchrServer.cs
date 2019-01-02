////////////////////////////////////////////////////////////////////////////////////////////////////////
//         Реализовать клиент-серверное приложение согласно варианту, приведенному в таблице 1        //
//             Server:  synchronous	                                                                  //
//             Client:  synchronous	                                                                  //
//         Реализовать возможность инициализировать параметры связи (адрес, порт) заданным способом.  //
//             Initializing input parameters:   file.txt                                              //
//         Обязательно осуществлять проверку на правильность введенных параметров.                    //
//         Использовать параметры по умолчанию, в случае возникновения исключений.                    //
////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
public class ServerClient
{
    public static void Main()
    {
        Color("Green", "Black");
        Console.WriteLine("********************************");
        Console.WriteLine("************ SERVER ************");
        Console.WriteLine("********************************");
        Color("Black", "Green");
        try
        {
            NetServer Server = new NetServer();
            while (true)
            {
                Server.WaitConnected();
                Server.WaitMessage();
                Server.SendMessage();
                Server.DisconnectClient();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("FATAL ERROR -> " + e.Message);
        }
        finally
        {
            Console.WriteLine("BAY BAY");
            Console.Read();
        }
    }
    public static void Color(string BackgroundColor, string ForegroundColor)
    {
        switch (BackgroundColor)
        {
            case "Black": Console.BackgroundColor = ConsoleColor.Black; break;
            case "Blue": Console.BackgroundColor = ConsoleColor.Blue; break;
            case "Gray": Console.BackgroundColor = ConsoleColor.Gray; break;
            case "Red": Console.BackgroundColor = ConsoleColor.Red; break;
            case "Green": Console.BackgroundColor = ConsoleColor.Green; break;
            default: Console.BackgroundColor = ConsoleColor.White; break;

        }
        switch (ForegroundColor)
        {
            case "Black": Console.ForegroundColor = ConsoleColor.Black; break;
            case "Blue": Console.ForegroundColor = ConsoleColor.Blue; break;
            case "Gray": Console.ForegroundColor = ConsoleColor.Gray; break;
            case "Red": Console.ForegroundColor = ConsoleColor.Red; break;
            case "Green": Console.ForegroundColor = ConsoleColor.Green; break;
            default: Console.ForegroundColor = ConsoleColor.White; break;
        }
    }
}

public class NetServer
{
    private Socket sListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private Socket handler;
    private string data = null;

    public NetServer()
    {
        IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddr = ipHost.AddressList[1];
        Console.WriteLine("Main IP " + ipAddr);
        Console.Write("Enter the port open (range 5000-11000) ");
        int Port = int.Parse(Console.ReadLine());
        if (Port > 11000 || Port < 6000)
        {
            Console.WriteLine("Trying to open not available port, you open to  defolt port 11000");
            Port = 11000;
        }
        IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, Port);
        sListener.Bind(ipEndPoint);
        sListener.Listen(1);
    }

    public void WaitConnected()
    {
        Console.WriteLine("Waiting for a connection ...");
        handler = sListener.Accept();
        Console.WriteLine("Connected " + handler.RemoteEndPoint);
    }

    public void WaitMessage()
    {
        data = null;
        byte[] bytes = new byte[1024];
        Console.WriteLine("Waiting message... ");
        int bytesRec = handler.Receive(bytes);
        data += Encoding.Default.GetString(bytes, 0, bytesRec);
        Console.WriteLine("Сlient said message -> " + data);
    }

    public void SendMessage()
    {
        string theReply = "Thank you for those " + data.Length.ToString() + " characters :)";
        byte[] msg = Encoding.Default.GetBytes(theReply);
        handler.Send(msg);
    }

    public void DisconnectClient()
    {
        handler.Shutdown(SocketShutdown.Both);
        Console.WriteLine("Disconnected " + handler.RemoteEndPoint);
        handler.Close();
    }
}
