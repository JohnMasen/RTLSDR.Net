using RTLSDR.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace RTLSDR.Core
{
    public class RadioConfig
    {
        public int Frequency { get; set; }
        public int SampleRate{ get; set; }
        public bool DirectSampling{ get; set; }
        public int FrequencyCorrection{ get; set; }
        public int Gain{ get; set; }
        public int BiasTee{ get; set; }
        public string ServerIP{ get; set; }
        public int ServerPort{ get; set; }

    }



    public class TCPReader : PipelineBase<RadioConfig, byte[]>
    {
        System.Net.Sockets.TcpClient client;
        CancellationToken token;
        public int BufferSize { get; private set; }
        private enum CommandType : byte
        {
            SetFrequency = 0x01,
            SetSampleRate = 0x02,
            SetGainMode = 0x03,
            SetGain = 0x04,
            SetFrequencyCorrection = 0x05,
            SetTestMode = 0x07,
            SetAGCMode = 0x08,
            SetDirectSampling = 0x09,
            SetTurnerGainIndex = 0x0d,
            SetBiasTee = 0x0e,
        }
        public TCPReader(int bufferSize = 16384) : base("TCPReader")
        {
            BufferSize = bufferSize;
        }

        public override void Start(IEnumerable<RadioConfig> source, CancellationToken token)
        {
            this.token = token;
            base.Start(source, token);
        }
        protected override void Init()
        {

        }
        private void SendCommand(CommandType command, int value)
        {
            byte[] valueBuffer = BitConverter.GetBytes(value);
            byte[] sendBuffer = new byte[5] { (byte)command, valueBuffer[3], valueBuffer[2], valueBuffer[1], valueBuffer[0] };
            client.GetStream().Write(sendBuffer, 0, 5);
        }

        protected override void doWork(RadioConfig source)
        {
            client = new System.Net.Sockets.TcpClient();
            client.Connect(source.ServerIP, source.ServerPort);

            #region Set Server Parameter
            SendCommand(CommandType.SetSampleRate, source.SampleRate);
            SendCommand(CommandType.SetFrequency, source.Frequency);
            SendCommand(CommandType.SetDirectSampling, source.DirectSampling ? 1 : 0);
            SendCommand(CommandType.SetFrequencyCorrection, source.FrequencyCorrection);
            SendCommand(CommandType.SetBiasTee, source.BiasTee);
            if (source.Gain == 0)
            {
                SendCommand(CommandType.SetGainMode, 0);
            }
            else
            {
                SendCommand(CommandType.SetGainMode, 1);
                SendCommand(CommandType.SetGain, source.Gain);
            }
            
            #endregion

            #region receive data
            byte[] buffer = new byte[BufferSize];
            int pos = 0;
            using (BinaryReader reader = new BinaryReader(client.GetStream()))
            {
                while (!token.IsCancellationRequested)
                {
                    int length = BufferSize - pos;
                    int bytesRead = reader.Read(buffer, pos, length);
                    pos += bytesRead;
                    if (pos == BufferSize ) //buffer full
                    {
                        byte[] tmp = new byte[BufferSize];
                        buffer.CopyTo(tmp.AsSpan());
                        pos = 0;
                        Result.Add(tmp);
                    }
                }
                
            }
            #endregion

        }
    }
}
