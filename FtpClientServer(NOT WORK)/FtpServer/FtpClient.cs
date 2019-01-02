//Сетевое программирование .Net для профессионалов с. 184
using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;

namespace Wrox.Networking.TCP.FtpUtil
{
    internal sealed class FtpClient
    {
        private const int bufferSize = 65536;
        private NetworkStream controlStream;
        private NetworkStream dataStream;
        private TcpClient client;
        private string username;
        private string password;
        private TcpClient dataClient = null;


        public FtpClient(string username, string password)
        {
            this.username = username;
            this.password = password;
        }

        public void Connect(string hostname)
        {
            //Устанавливаем закрытые поля, представляющие управляющее
            //TCP-соединение с сервером и NetworkStream для взаимодействия
            //с сервером
            client = new TcpClient(hostname, 21);
            controlStream = client.GetStream();

            string responseMessage;
            if (GetResponse(out responseMessage) != 220)
            {
                throw new WebException(responseMessage);
            }
            Logon(username, password);
        }

        public int GetResponse(out string responseMessage)
        {
            //Считываем ответ сервера, отбрасываем нули и возвращаем
            byte[] response = new byte[client.ReceiveBufferSize];
            controlStream.Read(response, 0, response.Length);
            responseMessage = Encoding.ASCII.GetString(response).Replace("\0", "");

            return int.Parse(responseMessage.Substring(0, 3));
        }

        private void Logon(string username, string password)
        {
            //Отправляем оманду USER FTP. Сервер должен ответить сообщением
            //331, запрашивая пароль пользователя.
            string respMessage;
            int resp = SendCommand("USER" + username, out respMessage);
            if (resp != 230)
            {
                //Отправляем команду PASS FTP. Сервер должен ответить сообщением
                //230, подтверждая вход в систему пользователя.\
                resp = SendCommand("PASS" + password, out respMessage);
                if (resp != 230)
                    throw new UnauthorizedAccessException("Unable to login to the FTP server");
            }
        }

        public NetworkStream GetReadStream(string filename, bool binaryMode)
        {
            return DownloadFile(filename, binaryMode);
        }

        public NetworkStream GetWriteStream(string filename, bool binaryMode)
        {
            return UploadFile(filename, binaryMode);
        }

        private NetworkStream DownloadFile(string filename, bool binaryMode)
        {
            if (dataClient == null)
                dataClient = CreateDataSocket();

            SetBinaryMode(binaryMode);
            string respMessage;
            int resp = SendCommand("RETR " + filename, out respMessage);

            if (resp != 150 && resp != 125)
                throw new WebException(respMessage);

            dataStream = dataClient.GetStream();
            return dataStream;
        }

        private NetworkStream UploadFile(string filename, bool binaryMode)
        {
            if (dataClient == null)
                dataClient = this.CreateDataSocket();

            //Устанавливаем режим: двоичный или ASCII
            SetBinaryMode(binaryMode);

            //Отправляем команду STOR для копирования файла на сервер
            string respMessage;
            int resp = SendCommand("STOP " + filename, out respMessage);

            //Мы должны получить ответ 150, означающий, что сервер
            //открывает соединение для данных
            if (resp != 150 && resp != 125)
                throw new WebException("Cannot upload files to server");

            dataStream = dataClient.GetStream();
            return dataStream;
        }

        private void SetBinaryMode(bool binaryMode)
        {
            int resp;
            string respMessage;

            if (binaryMode)
                resp = SendCommand("TYPE I", out respMessage);
            else
                resp = SendCommand("TYPE A", out respMessage);

            if (resp != 200)
                throw new WebException(respMessage);
        }

        private TcpClient CreateDataSocket()
        {
            //посылаем команду серверу - слушать на порте данных
            //(это не порт по умолчанию для данных) и ждать соединения
            string respMessage;
            int resp = SendCommand("PASV", out respMessage);
            if (resp != 227)
                throw new WebException(respMessage);

            //Ответ включает адрес хоста и номер порта
            //они разделены запятыми
            //Создаем IP-фдрес и номер порта
            int[] parts = new int[6];
            try
            {
                int index1 = respMessage.IndexOf('(');
                int index2 = respMessage.IndexOf(')');
                string endPointData = respMessage.Substring(index1 + 1,
                    index2 - index1 - 1);
                string[] endPointParts = endPointData.Split(',');
                for (int i = 0; i < 6; i++)
                {
                    parts[i] = int.Parse(endPointParts[i]);
                }
            }
            catch
            {
                throw new WebException("Malformed PASV reply: " + respMessage);
            }

            string ipAddress = parts[0] + "." + parts[1] + "." + parts[2] +
                "." + parts[3];
            int port = parts[4] + parts[5];   //(parts[4] < 8) + parts[5];

            //Создаем сокет клиента
            TcpClient dataClient = new TcpClient();

            //Соединяемся с портом данных сервера
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ipAddress), port);

                dataClient.Connect(remoteEP);
            }
            catch (Exception)
            {
                throw new WebException("Can't connect to remote server");
            }
            return dataClient;
        }

        public void Close()
        {
            if (dataStream != null)
            {
                dataStream.Close();
                dataStream = null;
            }

            string respMessage;
            GetResponse(out respMessage);

            Logoff();

            //Close the control TcpClient and NetworkStream
            controlStream.Close();
            client.Close();
        }

        public void Logoff()
        {
            //Отправляем команду QUIT, чтобы отключиться от сервера
            string respMessage;
            SendCommand("STAT", out respMessage); // Только тестирование
            GetResponse(out respMessage);         //У STAT 2 строки ответа!
            SendCommand("QUIT", out respMessage);
        }

        internal int SendCommand(string command, out string respMessage)
        {
            // Преобразуем строку команды (завершаемую символами CRLF)
            // в массив байтов и записываем ее в управляющий поток
            byte[] request = Encoding.ASCII.GetBytes(command + "\r\n");
            controlStream.Write(request, 0, request.Length);
            return GetResponse(out respMessage);
        }
    }
}

