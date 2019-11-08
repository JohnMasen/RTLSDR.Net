using Microsoft.Azure.Devices.Client;
using System.Text.Json;
using RTLSDR.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Security.Cryptography;

namespace WeatherStation2MQTT
{
    public class IoTHubDeviceSinkConfig
    {
        public string ConnectionString { get; set; }
    }
    internal class IoTHubDeviceSink : PipelineBase<MisolWeahterData, MisolWeahterData>
    {
        DeviceClient client;
        IoTHubDeviceSinkConfig config;
        bool enabled = false;
        protected override void Init()
        {
            base.Init();
            enabled = !string.IsNullOrWhiteSpace(config.ConnectionString);
            if (enabled)
            {
                Console.WriteLine("IotHub device client enabled");
            }
            else
            {
                Console.WriteLine("Config not found or invalid connection string, IoTHub devcie client disabled");
                return;
            }
            client = DeviceClient.CreateFromConnectionString(config.ConnectionString, TransportType.Amqp);
        }
        public IoTHubDeviceSink(IoTHubDeviceSinkConfig deviceSinkConfig):base(nameof(IoTHubDeviceSink))
        {
            config = deviceSinkConfig;
        }

        protected override async void doWork(MisolWeahterData source)
        {
            if (enabled)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(source));
                Message m = new Message(buffer);
                await client.SendEventAsync(m);
                Console.WriteLine($"{DateTime.Now}: Device Message Sent");
            }
            Result.Add(source);
        }
    }
}
