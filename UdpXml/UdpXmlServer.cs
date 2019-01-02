using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Xml.Serialization;
using System.Threading;

namespace UdpXml
{
    public class UdpXmlServer
    {
        private static FileDetails fileDet = new FileDetails();

        //Поля связаннвые с UdpClient
        private static IPAddress remoteIPAddress;
        private const int remotePort = 5002;
        private static UdpClient sender = new UdpClient();
        private static IPEndPoint endPoint;
        //Объект FileStream
        private static FileStream fs;

        [Serializable]
        public class FileDetails
        {
            public string FILETYPE = "";
            public long FILESIZE = 0;
        }

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                //Получаем удаленный IP-адрес и создаем IPEndPoint
                Console.WriteLine("Enter Remote IP address");
                remoteIPAddress = IPAddress.Parse(Console.ReadLine());
                endPoint = new IPEndPoint(remoteIPAddress, remotePort);

                //Получаем путь файла. (Важно: размер файла должен быть менее 8К)
                Console.WriteLine("Enter File path and name to send. ");
                fs = new FileStream(@Console.ReadLine().ToString(), FileMode.Open, FileAccess.Read);

                if(fs.Length > 8192)
                {
                    Console.Write("This version transfers files with size < 8192 bytes");
                    sender.Close();
                    fs.Close();
                    return;
                }
                //Send info about file to client
                SendFileInfo();

                //Wait 2 sec
                Thread.Sleep(2000);

                //Send file
                SendFile();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }
        }

        public static void SendFileInfo()
        {
            //Get the file type and extension
            fileDet.FILETYPE = fs.Name.Substring((int)fs.Name.Length - 3, 3);
            //Get lenght this file
            fileDet.FILESIZE = fs.Length;

            XmlSerializer fileSerializer = new XmlSerializer(typeof(FileDetails));

            MemoryStream stream = new MemoryStream();

            //Serialize object
            fileSerializer.Serialize(stream, fileDet);

            //Read stream to byte
            stream.Position = 0;
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, Convert.ToInt32(stream.Length));

            Console.WriteLine("Sending file details...");

            //Send info about this file
            sender.Send(bytes, bytes.Length, endPoint);
            stream.Close();
        }

        private static void SendFile()
        {
            //Create filestream
            byte[] bytes = new byte[fs.Length];

            //Stream to byte
            fs.Read(bytes, 0, bytes.Length);

            Console.WriteLine("Sending file...size = " + fs.Length + " bytes");

            try
            {
                //send file
                sender.Send(bytes,bytes.Length, endPoint);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                //Clear object
                fs.Close();
                sender.Close();
            }
            Console.WriteLine("File sent successfully.");
            Console.ReadLine();
        }
    }

}
