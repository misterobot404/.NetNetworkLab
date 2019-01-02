using System;
using System.Text;
using System.Net;
using System.IO;
using System.Windows.Forms;

namespace ConvertMoneyHTTP
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            HttpWebRequest req;
            HttpWebResponse resp;
            StreamReader sr;

            char[] separator = { ',' };

            string result;
            string fullPath;
            string currencyFrom = "USD";
            string currencyTo = "INR";
            double amount = 100d;

            label1.Text = "Currency Converter";
            label2.Text = "Currency From : " + currencyFrom;
            label3.Text = "Currency To : " + currencyTo;
            label4.Text = "Amount : " + amount;

            // Построим URL, возвращающий котировку
            fullPath = "https://finance.yahoo.com/d/quotes.csv?s=" + currencyFrom +
                currencyTo + "X&f=sl1d1t1c1ohgv&e=.csv";

            try
            {
                req = (HttpWebRequest)WebRequest.Create(fullPath);
                resp = (HttpWebResponse)req.GetResponse();
                sr = new StreamReader(resp.GetResponseStream(), Encoding.ASCII);
                result = sr.ReadLine();
                resp.Close();
                string[] temp = result.Split(separator);

                if (temp.Length >1)
                {
                    double rate = Convert.ToDouble(temp[1]);
                    double convert = amount * rate;
                    label5.Text = amount + currencyFrom + convert + currencyTo;
                }
                else
                {
                    label5.Text = "Error in getting currency rates from website";
                }
            }
            catch (Exception)
            {
                label5.Text = "Error";
            }
        }
    }
}
