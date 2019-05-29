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
        System.Net.Sockets.TcpClient client;
        CancellationToken token;
        public TCPReader():base("TCPReader")
        {
        }

        public override void Start(IEnumerable<string> source, CancellationToken token)
        {
            this.token = token;
            base.Start(source, token);
        }
        protected override void Init()
        {
            
        }

        protected override void doWork(string source)
        {
            string[] addr = source.Split(",");
            client = new System.Net.Sockets.TcpClient(addr[0], int.Parse(addr[1]));
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
