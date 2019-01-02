using System;
using System.IO;
using System.Net;
using Wrox.Networking.TCP.FtpUtil;

namespace TestClient
{
    public class TestClient
    {
        const int bufferSize = 65536;

        static void Main(string[] args)
        {
            //Регистрируем схему ftp.
            //В качестве альтернативы можно использовать файл config
            WebRequest.RegisterPrefix("ftp", new FtpRequestCreator());
            UploadDemo();
            DownloadDemo();
        }

        public static void UploadDemo()
        {
            Wrox.Networking.TCP.FtpUtil.FtpWebRequest req = (Wrox.Networking.TCP.FtpUtil.FtpWebRequest)WebRequest.Create(
                "ftp://192.168.0.1/demofile.bmp");
            req.Username = "Administrator";
            req.Password = "secret";
            req.Method = "PUT"; //STOP или PUT
            req.BinaryMode = true;
            Stream writeStream = req.GetResponse().GetResponseStream();

            FileStream fs = new FileStream(@"c:\temp\cool.bmp", FileMode.Open);

            byte[] buffer = new byte[bufferSize];
            int read;
            while ((read = fs.Read(buffer, 0, bufferSize)) > 0)
            {
                writeStream.Write(buffer, 0, bufferSize);
            }
            writeStream.Close();
            fs.Close();
        }

        //Копируем файл с сервера, используя FtpWebRequest
        public static void DownloadDemo()
        {
            Wrox.Networking.TCP.FtpUtil.FtpWebRequest req = (Wrox.Networking.TCP.FtpUtil.FtpWebRequest)WebRequest.Create(
                "ftp://192.168.0.1/sample.bmp");
            // по умолчанию:
            /* req.Username = "anonymos";
            req.Password = "someuser@somemail.com";
            req.BinaryMode = true; 
            req.Method = "GET"; */

            Wrox.Networking.TCP.FtpUtil.FtpWebResponse resp = (Wrox.Networking.TCP.FtpUtil.FtpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();

            //Считываем двоичный файл
            FileStream fs = new FileStream(@"c:\temp\sample.bmp",
                FileMode.Create);
            byte[] buffer = new byte[bufferSize];
            int count;
            do
            {
                Array.Clear(buffer, 0, bufferSize);
                count = stream.Read(buffer, 0, bufferSize);
                fs.Write(buffer, 0, count);
            } while (count > 0);

            stream.Close();
            fs.Close();

            /* 
             Cчитываем текстовый файл
             StreamReader reader = new StreamSeader(stream);
             string line;
             while ((line = reead.ReadLine()) != null)
             {
                Console.WriteLine(line);
             }
             reader.Close(); 
             */
        }
    }
}
