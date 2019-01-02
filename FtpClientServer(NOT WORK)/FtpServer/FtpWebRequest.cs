// стр в учебнике 164(180)

using System;
using System.Net;

namespace Wrox.Networking.TCP.FtpUtil
{
    public class FtpWebRequest : WebRequest
    {
        private string username = "anonymous";
        internal string password = "someuser@somemail.com";
        private Uri uri;
        private bool binaryMode = true;
        private string method = "GET";

        internal FtpWebRequest(Uri uri)
        {
            this.uri = uri;
        }

        public string Username
        {
            get { return username; }
            set { username = value; }
        }

        public string Password
        {
            set { password = value; }
        }

        public bool BinaryMode
        {
            get { return binaryMode; }
            set { binaryMode = value; }
        }

        public override System.Uri RequestUri
        {
            get { return uri; }
        }

        public override string Method
        {
            get { return method; }
            set { method = value; }
        }

        public override System.Net.WebResponse GetResponse()
        {
            FtpWebResponse response = new FtpWebResponse(this);
            return response;
        }
    }
}
