// стр в учебнике 167(183)

using System;
using System.IO;
using System.Net.Sockets;

namespace Wrox.Networking.TCP.FtpUtil
{
    internal class FtpWebStream : Stream
    {
        private FtpWebResponse response;
        private NetworkStream dataStream;

        public FtpWebStream(NetworkStream networkStream, FtpWebResponse response)
        {
            this.dataStream = dataStream;
            this.response = response;
        }
        public override void Close()
        {
            response.Close();
            base.Close();
        }

        public override void Flush()
        {
            dataStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return dataStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotSupportedException("Seek not supported");
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("SetLenght not spported");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            dataStream.Write(buffer, offset, count);
        }

        public override bool CanRead
        {
            get { return dataStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return dataStream.CanWrite; }
        }

        public override long Length
        {
            get { throw new PlatformNotSupportedException("Lenght not supported"); }
        }

        public override long Position
        {
            get { throw new PlatformNotSupportedException("Position not supported"); }
            set { throw new PlatformNotSupportedException("Position not supported"); }
        }
    }
}