// стр в учебнике 165(181)

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Wrox.Networking.TCP.FtpUtil
{
    public class FtpWebResponse : WebResponse
    {
        private FtpWebRequest request;
        private FtpClient client;

        internal FtpWebResponse(FtpWebRequest request)
        {
            this.request = request;
        }

        public override Stream GetResponseStream()
        {
            // Разбиваем URI, чтобы получить имя хоста и имя файла
            string hostname;
            string filename;
            GetUriComponents(request.RequestUri.ToString(), out hostname,
                out filename);

            // Соединяемся с FTP-сервером и получаем поток
            client = new FtpClient(request.Username, request.password);
            client.Connect(hostname);

            NetworkStream dataStream = null;
            switch (request.Method)
            {
                case "GET":
                case "Retr":
                    dataStream = client.GetReadStream(filename, request.BinaryMode);
                    break;

                case "PUT":
                case "STOP":
                    dataStream = client.GetWriteStream(filename, request.BinaryMode);
                    break;

                default:
                    throw new WebException("Method " + request.Method + " not supported");
            }

            FtpWebStream ftpStream = new FtpWebStream(dataStream, this);
            return ftpStream;
        }


        private void GetUriComponents(string uri, out string hostname, out string filename)
        {
            uri = uri.ToLower();
            if (uri.Length < 7)
                throw new UriFormatException("Invalid URI");

            if (uri.Substring(0, 6) != "ftp://")
                throw new NotSupportedException("Only FTP requests arw supported");
            else
                uri = uri.Substring(6, uri.Length - 6);

            string[] uriParts = uri.Split(new char[] { '/' }, 2);
            if (uriParts.Length != 2)
                throw new UriFormatException("Invalid URI");

            hostname = uriParts[0];
            filename = uriParts[1];
        }

        public override void Close()
        {
            client.Close();
        }
    }
}
