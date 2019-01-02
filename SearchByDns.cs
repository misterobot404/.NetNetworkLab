using System;
using System.Net;

namespace ConsoleApplication8
{
    class AsyncDnsDemo
    {
        private static string hostname = "www.wrox.com";
        static void Main(string[] args)
        {
            if (args.Length != 0)
                hostname = args[0];
            Dns.BeginGetHostByName(hostname, new AsyncCallback(DnsLookupCopleted), null);
            Console.WriteLine("Waiting for the results...");
            Console.ReadLine();
        }
        private static void DnsLookupCopleted(IAsyncResult ar)
        {
            IPHostEntry entry = Dns.EndGetHostByName(ar);

            Console.WriteLine("IP Addresses for 0: ", hostname);
            foreach (IPAddress address in entry.AddressList)
                Console.WriteLine(address.ToString());
            Console.WriteLine("\nAlias names: ");
            foreach (string alisName in entry.Aliases)
                Console.WriteLine(alisName);
            Console.WriteLine("\nAnd the real hostname: ");
            Console.WriteLine(entry.HostName);
        }
    }
}