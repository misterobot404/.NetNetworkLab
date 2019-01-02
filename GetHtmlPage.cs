using System;
using System.Net;
using System.IO;


namespace ConsoleApplication8
{
    class AsyncDnsDemo
    {

        static void Main(string[] args)
        {
            Uri uri = new Uri("http://www.wrox.com");
            WebRequest request = WebRequest.Create(uri);
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
            response.Close();
            reader.Close();
            Console.ReadLine();
        }
    }
}