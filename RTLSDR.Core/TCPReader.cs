using RTLSDR.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace RTLSDR.Core
{
    public struct RadioConfig
    {
        public int Frequency;
        public int SampleRate;
        public bool DirectSampling;
        public int FrequencyCorrection;
        public int Gain;
        public int BiasTee;
        public string ServerIP;
        public int ServerPort;
        
    }
    


    public class TCPReader:PipelineBase<RadioConfig,byte[]>
    {
        System.Net.Sockets.TcpClient client;
        CancellationToken token;
        
        private enum CommandType:byte
        {
            SetFrequency=0x01,
            SetSampleRate=0x02,
            SetGainMode=0x03,
            SetGain=0x04,
            SetFrequencyCorrection=0x05,
            SetTestMode=0x07,
            SetAGCMode=0x08,
            SetDirectSampling=0x09,
            SetTurnerGainIndex=0x0d,
            SetBiasTee=0x0e,
        }
        public TCPReader():base("TCPReader")
        {
        }

        public override void Start(IEnumerable<RadioConfig> source, CancellationToken token)
        {
            this.token = token;
            base.Start(source, token);
        }
        protected override void Init()
        {
            
        }
        private void SendCommand(CommandType command,int value)
        {
            byte[] valueBuffer = BitConverter.GetBytes(value);
            byte[] sendBuffer = new byte[5] { (byte)command, valueBuffer[3], valueBuffer[2], valueBuffer[1], valueBuffer[0] };
            client.GetStream().Write(sendBuffer,0,5);
        }

        protected override void doWork(RadioConfig source)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            client = new System.Net.Sockets.TcpClient();
            client.Connect(source.ServerIP, source.ServerPort);
            SendCommand(CommandType.SetSampleRate, source.SampleRate);
            SendCommand(CommandType.SetFrequency, source.Frequency);

            SendCommand(CommandType.SetDirectSampling, source.DirectSampling ? 1 : 0);
            SendCommand(CommandType.SetFrequencyCorrection, source.FrequencyCorrection);
            if (source.Gain == 0)
            {
                SendCommand(CommandType.SetGainMode, 0);
            }
            else
            {
                SendCommand(CommandType.SetGainMode, 1);
                SendCommand(CommandType.SetGain, source.Gain);
            }

            SendCommand(CommandType.SetBiasTee, source.BiasTee);


            byte[] header = new byte[12];
            using (BinaryReader reader = new BinaryReader(client.GetStream()))
            {
                //int count = 16;
                reader.Read(header);
                
                sw.Start();
                while (!token.IsCancellationRequested)
                {
                    byte[] buffer = new byte[1024 * 16];
                    reader.Read(buffer);
                    //if (sw.ElapsedMilliseconds>=1000)
                    //{
                    //    Console.WriteLine($"{count} KB received in 1 s");
                    //    count = 0;
                    //    sw.Restart();
                    //}
                    //count += 16;
                    Result.Add(buffer);
                }
            }
        }
    }
}
