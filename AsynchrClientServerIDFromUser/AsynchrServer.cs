 *  ����������� ������-��������� ���������� �������� ��������, ������������ � ������� 1.
 * Server:  asynchronous	
 * Client:  synchronous	
    ����������� ����������� ���������������� ��������� ����� (�����, ����) �������� ��������.
 * Initializing input parameters:   interface with user
    ����������� ������������ �������� �� ������������ ��������� ����������. 
    ������������ ��������� �� ���������, � ������ ������������� ����������.
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public static class AsyncServer
{
    //����� ��� ��������� � �������� ������
    public static byte[] buffer = new byte[1024];
    //����� ������� ��� ��������� �������������
    public static ManualResetEvent socketEvent = new ManualResetEvent(false);
    public static void Main(string[] args)
    {

        IPHostEntry ipHost = Dns.Resolve(Dns.GetHostName());
        int num = 0;
        foreach (var item in ipHost.AddressList)
        {
            Console.WriteLine(++num + ". " + item.ToString());
        }
        num = 1;
        Console.WriteLine("Select ip ");
        int TIP = int.Parse(Console.ReadLine());
        string TR = " ";
        foreach (var item in ipHost.AddressList)
        {
            if (TIP == num)
            {
                Console.WriteLine("You selected " + item.ToString());
                TR = item.ToString();
                break;
            }
            else ++num;
        }
        if (TIP > num || TIP <= 0)
        {
            Console.WriteLine("Input error, you connect to  defolt ip");
            TR = ipHost.AddressList[0].ToString();
            Console.WriteLine("You selected " + ipHost.AddressList[0].ToString());
        }
        Console.WriteLine("Enter the port open (recommended 5000-11000)");
        int Port = int.Parse(Console.ReadLine());


        IPEndPoint localEnd = new IPEndPoint(IPAddress.Parse(TR), Port);

        //������ ����� Tcp/Ip
        Socket sListener = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);

        //��������� �����
        sListener.Bind(localEnd);

        //�������� ������� ����������
        int T=30;
        sListener.Listen(T);

        AsyncCallback aCallback = new AsyncCallback(AcceptCallback);
        Console.WriteLine("Waiting for a connection...");

        //������������ �������, ������ �������� �� ����������
        for(int i=0;i!= T; ++i)
        {
            sListener.BeginAccept(aCallback, sListener);
        }
        
        

        //��� ���� ������ ������ �������� ������
        socketEvent.WaitOne();
        Console.ReadLine();
    }
    public static void AcceptCallback(IAsyncResult ar)
    {
        //����� ��� ��������� ��������
        Socket listener = (Socket)ar.AsyncState;
        //����� �����
        Socket handler = listener.EndAccept(ar);
        Console.WriteLine("Connecting " + handler.RemoteEndPoint);

        handler.BeginReceive(buffer, 0, buffer.Length, 0,
            new AsyncCallback(ReceiveCallback), handler);
    }
    public static void ReceiveCallback(IAsyncResult ar)
    {
        string content = String.Empty;
        Socket handler = (Socket)ar.AsyncState;
        Console.WriteLine("From User ID:" +
    handler.RemoteEndPoint);
        int bytesRead = handler.EndReceive(ar);

        //esli dannie est
        if (bytesRead > 0)
        {
            //������������ �� � �������� ������
            content += Encoding.Default.GetString(buffer, 0, bytesRead);
            //���� �� ������� ������ ����� ���������...
                Console.WriteLine("Read {0} bytes from socket. \n Data: {1}",
                    content.Length, content);
                byte[] byteData = Encoding.Default.GetBytes(content);
                //���������� ������ ������� �������
                handler.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), handler);
        }
    }
    public static void SendCallback(IAsyncResult ar)
    {
        Socket handler = (Socket)ar.AsyncState;

        //���������� ������ ������� �������
        int bytesSent = handler.EndSend(ar);
        Console.WriteLine("Sent {0} bytes to Client.", bytesSent);

        //��������� �����
        handler.Shutdown(SocketShutdown.Both);
        handler.Close();
        //������������� ������� ��� ��������� ������
        socketEvent.Set();
    }
}
