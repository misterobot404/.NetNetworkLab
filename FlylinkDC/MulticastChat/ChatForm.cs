using System;
using System.Configuration;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO;

namespace MulticastChat
{
    public partial class ChatForm : Form
    {
        private IPAddress groupAddress;     // Групповой адрес рассылки сообщений
        private bool stopListener = true;   // Флаг остановки слуш. потока
        public string name;                // Имя пользователя в разговоре
        private UnicodeEncoding encoding = new UnicodeEncoding(); // Формирование строки
        private string message;             // Сообщение для отправки

        // Chat
        private UdpClient clientMsg;        // Сокет клиента сообщений      
        private int localPortMsg;           // Локальный порт для приёма сообщений
        private int remotePortMsg;          // Удаленный порт для отправки сообщений
        private IPEndPoint remoteEPMsg;     // Удаленная точка для отправки сообщения
        private int ttl;                    // Время жизни udp пакета

        // File Info
        private UdpClient clientFileInfo;   // Сокет клиента  для сообщений о файлах
        private int localPortFileInfo;      // Локал порт для приёма инф о файлах
        private int remotePortFileInfo;     // Удаленный порт для отправки инф о файлах
        private IPEndPoint remoteEPFileInfo;    // Удаленная точка для отправки сообщения
        private FileDetails fileDet = new FileDetails();  // Класс содержащий информацию о файле
        private IPEndPoint fromEP;

        [Serializable]
        public class FileDetails
        {
            public string NAME = "";
            public string FILEPATH = "";
            public string FILENAME = "";
            public string FILESIZE = "";
            public string IP = "";
        }

        // Download File
        private int localPortDownloadFile;  // Локал порт для приёма файлов
        private int remotePortDownloadFile; // Удаленный порт для отправки файлов
        private IPAddress hostIp;           // Мой локальный IP
        private UdpClient server;           // Сокет слушающий подключения
        private UdpClient client;           // Сокет для отправки файлов
        private IPEndPoint remoteEPFile;    // Удаленная точка для отправки файла

        public ChatForm()
        {
            InitializeComponent();
            try
            {
                // Считываем конфигурационный файл приложения
                NameValueCollection configuration = ConfigurationSettings.AppSettings;
                groupAddress = IPAddress.Parse(configuration["GroupAddress"]);

                localPortMsg = int.Parse(configuration["LocalPortMsg"]);
                localPortFileInfo = int.Parse(configuration["LocalPortFileInfo"]);
                localPortDownloadFile = int.Parse(configuration["LocalDownloadFile"]);

                remotePortMsg = int.Parse(configuration["RemotePortMsg"]);
                remotePortFileInfo = int.Parse(configuration["RemotePortFileInfo"]);
                remotePortDownloadFile = int.Parse(configuration["RemotePortDownloadFile"]);

                // Получение имени компьютера.
                String host = Dns.GetHostName();
                // Получение ip-адреса.
                hostIp = Dns.GetHostByName(host).AddressList[0];

                client = new UdpClient(localPortDownloadFile);
                server = new UdpClient(remotePortDownloadFile);

                ttl = int.Parse(configuration["TTL"]);
            }
            catch
            {
                MessageBox.Show(this, "Error in application configuration file!", "Error Multicast Chat", MessageBoxButtons.OK, MessageBoxIcon.Error);
                buttonStart.Enabled = false;
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            name = textName.Text;
            try
            {
                stopListener = false;

                // Присоед к группе рассылки сообщений
                clientMsg = new UdpClient(localPortMsg);
                clientMsg.JoinMulticastGroup(groupAddress, ttl);
                remoteEPMsg = new IPEndPoint(groupAddress, remotePortMsg);

                Thread receiver = new Thread(new ThreadStart(ListenerMessage));
                receiver.IsBackground = true;
                receiver.Start();

                byte[] data = encoding.GetBytes(name + " has joined the chat");
                clientMsg.Send(data, data.Length, remoteEPMsg);

                // Присоед к группе рассылки информации о файлах
                clientFileInfo = new UdpClient(localPortFileInfo);
                clientFileInfo.JoinMulticastGroup(groupAddress, ttl);
                remoteEPFileInfo = new IPEndPoint(groupAddress, remotePortFileInfo);

                Thread receiver2 = new Thread(new ThreadStart(ListenerFileInfo));
                receiver2.IsBackground = true;
                receiver2.Start();

                // Поток ожидающий скачивания файлов
                
                Thread receiver3 = new Thread(new ThreadStart(ListenerFile));
                receiver3.IsBackground = true;
                receiver3.Start();


                unblockButtons();
            }
            catch (SocketException ex)
            {
                stopListener = true;
                MessageBox.Show(this, ex.Message, "Error MulticastChat", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void buttonStop_Click(object sender, EventArgs e)
        {
            StopListener();
        }

        // Chat
        private void ListenerMessage()
        {
            try
            {
                while (!stopListener)
                {
                    IPEndPoint ep = null;

                    byte[] buffer = clientMsg.Receive(ref ep);
                    message = encoding.GetString(buffer);

                    this.Invoke(new MethodInvoker(DisplayReceivedMessage));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error MulticastChat", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void buttonSend_Click(object sender, EventArgs e)
        {
            try
            {
                // Отправка сообщение группе
                byte[] data = encoding.GetBytes(name + ": " + textMessage.Text);
                clientMsg.Send(data, data.Length, remoteEPMsg);
                textMessage.Clear();
                textMessage.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error MulticastChar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void DisplayReceivedMessage()
        {
            string time = DateTime.Now.ToString("t");
            textMessages.Text = time + " " + message + "\r\n" + textMessages.Text;

            statusStrip1.Text = "Received last message at " + time;
        }

        // File Info
        private void ListenerFileInfo()
        {
            try
            {
                while (!stopListener)
                {
                    IPEndPoint ep2 = null;

                    byte[] buffer = clientFileInfo.Receive(ref ep2);

                    if (buffer.Length == 0) return;

                    XmlSerializer fileSerializer = new XmlSerializer(typeof(FileDetails));
                    MemoryStream stream1 = new MemoryStream();

                    //Полученные байты - в поток
                    stream1.Write(buffer, 0, buffer.Length);
                    stream1.Position = 0;

                    //Вызываем метод Deserialize и приводим к типу объекта
                    fileDet = (FileDetails)fileSerializer.Deserialize(stream1);
                    fileDet.IP = ep2.Address.ToString();
                    stream1.Close();

                    this.Invoke(new MethodInvoker(DisplayReceivedListView));

                    message = fileDet.NAME + ": расшарил файл " + fileDet.FILENAME +
                        " размером " + fileDet.FILESIZE.ToString() + " байт";

                    this.Invoke(new MethodInvoker(DisplayReceivedMessage));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error ListenerFileInfo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void buttonPush_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog OPF = new OpenFileDialog();
                if (OPF.ShowDialog() == DialogResult.OK)
                {
                    FileInfo fi = new FileInfo(OPF.FileName);
                    fileDet.FILENAME = fi.Name;
                    fileDet.NAME = name;                                 
                    fileDet.FILESIZE = fi.Length.ToString();
                    fileDet.FILEPATH = OPF.FileName;

                    XmlSerializer fileSerializer = new XmlSerializer(typeof(FileDetails));
                    MemoryStream stream = new MemoryStream();
                    fileSerializer.Serialize(stream, fileDet);

                    //Read stream to byte
                    stream.Position = 0;
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, Convert.ToInt32(stream.Length));
                    stream.Close();

                    //Send info about this file
                    clientFileInfo.Send(bytes, bytes.Length, remoteEPFileInfo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error SendFileInfo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void DisplayReceivedListView()
        {
            ListViewItem items = new ListViewItem(fileDet.FILENAME);
            items.SubItems.Add(fileDet.NAME);
            items.SubItems.Add(fileDet.FILESIZE.ToString());
            items.SubItems.Add(fileDet.IP);
            items.SubItems.Add(fileDet.FILEPATH);
            listView1.Items.Add(items);
        }      

        // Download File
        private void ListenerFile()
        {
            try
            {
                while (!stopListener)
                {
                    // получаем путь к файлу
                    getFileName();

                    // Send file
                    SendFile();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error ListenerFileInfo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void buttonPullPage_Click(object sender, EventArgs e)
        {
            bool select = false;

            for (int i = 0; i < listView1.Items.Count; i++)
                if (this.listView1.Items[i].Focused == true) select = true;

            if (select)
            {                            
                SendFileName();
                
                ReceiveFile();

                MessageBox.Show("Файл загружен в директорию программы", "FlylinkDC++", MessageBoxButtons.OK);
            }
        }

        private void getFileName()
        {
            remoteEPFile = null;
            byte[] buffer = server.Receive(ref remoteEPFile);

            fromEP = remoteEPFile;

            XmlSerializer fileSerializer = new XmlSerializer(typeof(FileDetails));
            MemoryStream stream1 = new MemoryStream();

            //Полученные байты - в поток
            stream1.Write(buffer, 0, buffer.Length);
            stream1.Position = 0;

            //Выываем метод Deserialize и приводим к типу объекта
            fileDet = (FileDetails)fileSerializer.Deserialize(stream1);

        }
        private void SendFile()
        {
            FileStream fs = new FileStream(@fileDet.FILEPATH, FileMode.Open, FileAccess.Read);

            byte[] bytes = new byte[fs.Length];

            //Stream to byte
            fs.Read(bytes, 0, bytes.Length);

            server.Send(bytes, bytes.Length, fromEP);
        }

        private void SendFileName()
        {
            fileDet.IP = listView1.FocusedItem.SubItems[3].Text;
            fileDet.NAME = listView1.FocusedItem.SubItems[1].Text;
            fileDet.FILENAME = listView1.FocusedItem.SubItems[0].Text;
            fileDet.FILESIZE = listView1.FocusedItem.SubItems[2].Text;
            fileDet.FILEPATH = listView1.FocusedItem.SubItems[4].Text;

            XmlSerializer fileSerializer = new XmlSerializer(typeof(FileDetails));
            MemoryStream stream = new MemoryStream();
            fileSerializer.Serialize(stream, fileDet);

            //Read stream to byte
            stream.Position = 0;
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, Convert.ToInt32(stream.Length));
            stream.Close();

            //Send info about this file
            remoteEPFile = new IPEndPoint(IPAddress.Parse(fileDet.IP), remotePortDownloadFile);

            client.Send(bytes, bytes.Length, remoteEPFile);
        }
        private void ReceiveFile()
        {
            remoteEPFile = null;

            //Получаем файл
            byte[] bytes = client.Receive(ref remoteEPFile);

            //Создаём файk temp с полученным расширением
            FileStream fs = new FileStream(fileDet.FILENAME, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            fs.Write(bytes, 0, bytes.Length);

            fs.Close();
        }

        private void StopListener()
        {
            // Отправляем группе сообщение о выходе
            byte[] data = encoding.GetBytes(name = " has left the chat");
            clientMsg.Send(data, data.Length, remoteEPMsg);

            // Покидаем группу
            clientMsg.DropMulticastGroup(groupAddress);
            clientMsg.Close();

            //Send info about this file
            data = encoding.GetBytes("");
            clientFileInfo.Send(data, data.Length, remoteEPFileInfo);

            // Покидаем группу
            clientFileInfo.DropMulticastGroup(groupAddress);
            clientFileInfo.Close();

            //
            clientFileInfo.Close();

            // Останавливаем поток, получающий сообщения
            stopListener = true;

            blockButtons();
        }
        private void ChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!stopListener)
                StopListener();
        }

        private void unblockButtons()
        {
            buttonStart.Enabled = false;
            buttonStop.Enabled = true;
            buttonSend.Enabled = true;
            textName.ReadOnly = true;
            buttonPullPage.Enabled = true;
            buttonPush.Enabled = true;
            textMessage.Enabled = true;
            textMessage.Text = "";
            textMessages.Enabled = true;
            label2.Enabled = true;
            listView1.Enabled = true;
        }
        private void blockButtons()
        {
            listView1.Enabled = false;
            buttonStart.Enabled = true;
            buttonStop.Enabled = false;
            buttonSend.Enabled = false;
            textName.ReadOnly = false;
            buttonPullPage.Enabled = false;
            buttonPush.Enabled = false;
            textMessage.Enabled = false;
            textMessages.Enabled = false;
            label2.Enabled = false;
        }   
    }
}
