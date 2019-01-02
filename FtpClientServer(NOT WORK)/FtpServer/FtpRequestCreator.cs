// cтр в учебнике 163(179)

using System;
using System.Net;

namespace Wrox.Networking.TCP.FtpUtil
{
    public class FtpRequestCreator : IWebRequestCreate
    {
        public FtpRequestCreator()
        {
        }
        public System.Net.WebRequest Create(System.Uri uri)
        {
            return new FtpWebRequest(uri);
        }
    }
}