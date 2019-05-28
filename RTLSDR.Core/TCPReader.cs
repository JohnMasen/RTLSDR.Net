using RTLSDR.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace RTLSDR.Core
{
    public class TCPReader:PipelineBase<string,byte[]>
    {
        System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient();
        public TCPReader(string address,int port):base("TCPReader")
        {
            client = new System.Net.Sockets.TcpClient(address, port);
        }

        protected override void doWork(IEnumerable<string> source, CancellationToken token)
        {
            byte[] header = new byte[12];
            using (BinaryReader reader = new BinaryReader(client.GetStream()))
            {
                reader.Read(header);
                byte[] buffer = new byte[1024 * 16];
                while (!token.IsCancellationRequested)
                {
                    reader.Read(buffer);
                    Result.Add(buffer);
                }
            }
        }
    }
}
