
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class ClientServer
{
    public static void Main()
    {
        Color("Red", "Black");
        Console.WriteLine("********************************");
        Console.WriteLine("************ CLIENT ************");
        Console.WriteLine("********************************");
        Color("Black", "Red");
        try
        {
            Console.WriteLine("Select ip ");
            string IP = Console.ReadLine();
            Console.WriteLine("Select port ");
            int Port = int.Parse(Console.ReadLine());

            NetClient Client = new NetClient();
            Client.ConnectServer(IP, Port);

            Console.Write("Write message -> ");
            string theMessage = Console.ReadLine();
            Client.SendMessage(theMessage);

            Client.GetMessage();

            Client.Disconnect();
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

public class NetClient
{
    private Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    public void ConnectServer(string ip, int port)
    {
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ip),port);
        sender.Connect(ipEndPoint);
        Console.WriteLine("Ñonnected " + sender.RemoteEndPoint.ToString());
    }

    public void SendMessage(string theMessage)
    {
        byte[] msg = Encoding.Default.GetBytes(theMessage);
        int bytesSend = sender.Send(msg);
    }

    public void GetMessage()
    {
        byte[] bytes = new byte[1024];
        int bytesRec = sender.Receive(bytes);
        Console.WriteLine("Server said message -> " + Encoding.Default.GetString(bytes, 0, bytesRec));
    }

    public void Disconnect()
    {
        Console.WriteLine("Disconnected " + sender.RemoteEndPoint.ToString());
        sender.Shutdown(SocketShutdown.Both);
        sender.Close();
    }
}
