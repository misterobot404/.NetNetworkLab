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
            DatabaseIpPort Database = new DatabaseIpPort();
            Database.GetData();
            Database.Show();

            Console.Write("Connect to ");
            int Number = int.Parse(Console.ReadLine());
            NetClient Client = new NetClient();
            Client.ConnectServer(Database.GetIP(Number), Database.GetPort(Number));

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

public class DatabaseIpPort
{
    private List<string> IP = new List<string>();
    private List<int> PORT = new List<int>();
    private string path;

    public void GetData()
    {
        Console.Write("The path to the file ");
        path = Console.ReadLine();
        File.ReadAllLines(path).ToList().ForEach(s =>
        {
            string[] arr = s.Split(':');
            IP.Add(arr[0]);
            PORT.Add(int.Parse(arr[1]));
        });
    }
    public void Show()
    {
        string[] strok = File.ReadAllLines(path);
        for (int i = 0; i < strok.Length; i++)
        {
            Console.WriteLine(i + " -> " + IP[i] + ":" + PORT[i]);
        }
    }

    public List<string> GetIP()
    {
        return IP;
    }
    public string GetIP(int T)
    {
        return IP[T];
    }

    public List<int> GetPort()
    {
        return PORT;
    }
    public int GetPort(int T)
    {
        return PORT[T];
    }

}

public class NetClient
{
    private Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    public void ConnectServer(string ip, int port)
    {
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ip),port);
        sender.Connect(ipEndPoint);
        Console.WriteLine("Сonnected " + sender.RemoteEndPoint.ToString());
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
