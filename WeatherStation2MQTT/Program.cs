using Microsoft.Extensions.Configuration;
using RTLSDR.Core;
using System;
using System.Collections.Generic;
using System.Threading;

namespace WeatherStation2MQTT
{
    class Program
    {
        private const int SIGNAL_LENGTH = 100;
        static CancellationTokenSource cts = new CancellationTokenSource();
        static void Main(string[] args)
        {
            Dictionary<int, byte> map = new Dictionary<int, byte>
            {
                [0]=0x0,
                [1]=0x1
            };
            //map.Add(0, 0);
            //map.Add(1, 1);
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("config.json", true, false)
                .Build();
            var c = config.GetSection("rtl_tcp").Get<RadioConfig>();

            RadioConfig[] server = new RadioConfig[] { c };

            TCPReader reader = new TCPReader();
            var node = reader.Chain(new SignalPreProcessor() { IQOutput = IQOutputEnum.IChannel })
                    .Chain(new IQ2Wave(c.Frequency, c.SampleRate))
                    .Chain(new LPF(c.Frequency, c.SampleRate, 1000f))
                    .Chain(new MoveAverage())
                    .Chain(new SignalCompare(0.2f))
                    .Chain(new SampleCounter())
                    .Chain(new SignalToByteArray(map, SIGNAL_LENGTH))
                    .Chain(new MorseDecode())
                    .Chain(new SignalReverse())
                    .Chain(new MisolWeatherStationDecoder())
                    .Chain(new MQTTSink(config.GetSection("mqtt")?.Get<MQTTConfig>()))
                    .Chain(new IoTHubDeviceSink(config.GetSection("iothub_device")?.Get<IoTHubDeviceSinkConfig>()))
                    ;

            reader.Start(server, cts.Token);
            Console.WriteLine("Application Started, Press Enter to exit");
            Console.ReadLine();
            cts.Cancel();
            Console.WriteLine("waiting for threads exit...");
            PipelineManager.Default.WaitAllExit();
            Console.WriteLine("done");
            Environment.Exit(0);
        }
    }
}
