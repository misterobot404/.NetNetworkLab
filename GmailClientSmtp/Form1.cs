using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GmailClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //
        }

        private static async Task SendEmailAsync(string yourMail, string yourPassword, string yourName, string toMail, string themeMsg, string textMsg)
        {
            try
            {
            // отправитель - устанавливаем адрес и отображаемое в письме имя
            MailAddress from = new MailAddress(yourMail, yourName);
            // кому отправляем
            MailAddress to = new MailAddress(toMail);
            // создаем объект сообщения
            MailMessage m = new MailMessage(from, to);
            // тема письма
            m.Subject = themeMsg;
            // текст письма
            m.Body = textMsg;
            // адрес smtp-сервера и порт, с которого будем отправлять письмо
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                // логин и пароль
                Credentials = new NetworkCredential(yourMail, yourPassword),
                EnableSsl = true
            };
            await smtp.SendMailAsync(m);
            MessageBox.Show("Сообщение отправлено :>", "Gmail", MessageBoxButtons.OK);
            }
            catch
            {
                MessageBox.Show("Ошибка вводе данных :<", "Gmail", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SendEmailAsync(textBox1.Text, textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text, textBox6.Text).GetAwaiter();
        }
    }
}
