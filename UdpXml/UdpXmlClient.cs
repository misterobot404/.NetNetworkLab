using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Net.Sockets;
using System.Xml.Serialization;

namespace UdpXmlClient
{
    public class UdpXmlClient
    {
        private static FileDetails fileDet;

        //Переменные UdClient
        private static int localPort = 5002;
        private static UdpClient receivingUdpClient = new UdpClient(localPort);
        private static IPEndPoint RemoteIpEndPoint = null;

        private static FileStream fs;
        private static byte[] receiveBytes = new byte[0];

        [Serializable]
        public class FileDetails
        {
            public string FILETYPE = "";
            public long FILESIZE = 0;
        }

        [STAThread]
        static void Main(string[] args)
        {
            //получаем информацию о файле
            GetFileDetails();

            //получаем файл
            ReceiveFile();
        }

        private static void GetFileDetails()
        {
            try
            {
                Console.WriteLine("-----***** Waiting to get File Details! *****-------");
                //Получаем информацию о файле
                receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);

                Console.WriteLine("--Received File Details!--");

                XmlSerializer fileSerializer = new XmlSerializer(typeof(FileDetails));
                MemoryStream stream1 = new MemoryStream();

                //Полученные байты - в поток
                stream1.Write(receiveBytes, 0, receiveBytes.Length);
                stream1.Position = 0;

                //Выываем метод Deserialize и приводим к типу объекта
                fileDet = (FileDetails)fileSerializer.Deserialize(stream1);
                Console.WriteLine("Received file of type ." + fileDet.FILETYPE + " whose size is " + fileDet.FILESIZE.ToString() + " bytes");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void ReceiveFile()
        {
            try
            {
                Console.WriteLine("----****Waiting to get File!****-----");

                //Получаем файл
                receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);

                //Преобрауем и отображаем данные
                Console.WriteLine("--File received....Saved...");

                //Создаём файk temp с полученным расширением
                fs = new FileStream("temp." + fileDet.FILETYPE, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                fs.Write(receiveBytes, 0, receiveBytes.Length);

                Console.WriteLine("--File Saved...");
                Console.WriteLine("---Opening file wirh associated rogram---");

                Process.Start(fs.Name); //Открываем файл связанной с ним программой
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                fs.Close();
                receivingUdpClient.Close();
            }
        }
    }
}
