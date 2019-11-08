using Microsoft.Extensions.Logging;
using RTLSDR.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using uPLibrary.Networking.M2Mqtt;

namespace WeatherStation2MQTT
{

    public class MQTTConfig
    {
        public string BrokerAddress { get; set; }
        public int BrokerPort { get; set; } = 1883;
        public string MessagePath { get; set; } = "/Devices/Weatherstation";
        public string UserName { get; set; }
        public string Password { get; set; }

    }
    class MQTTSink : PipelineBase<MisolWeahterData, MisolWeahterData>
    {
        MqttClient client;
        MQTTConfig config;
        private bool enabled = false;
        protected override void Init()
        {
            base.Init();
            enabled = !string.IsNullOrWhiteSpace(config.BrokerAddress);
            if (enabled)
            {
                Console.WriteLine("MQTT enabled");
            }
            else
            {
                Console.WriteLine("Config not found or invalid broker address, MQTT disabled");
                return;
            }

            if (!config.MessagePath.EndsWith("/"))
            {
                config.MessagePath += "/";
            }
            client = new MqttClient(config.BrokerAddress, config.BrokerPort, false, MqttSslProtocols.None, null, null);
            client.Connect("MQTTSink",config.UserName,config.Password);
            client.MqttMsgPublished += Client_MqttMsgPublished;
        }

        

        private void Client_MqttMsgPublished(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishedEventArgs e)
        {
            Console.WriteLine("Message sent");
        }

        public MQTTSink(MQTTConfig sinkConfig):base(nameof(MQTTSink))
        {
            config = sinkConfig;
            
        }
        protected override void doWork(MisolWeahterData source)
        {
            if (enabled)
            {
                string path = $"{config.MessagePath}{source.DeviceID}";
                byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(source));
                client.Publish(path, buffer);
            }
            
            Result.Add(source);
        }

        
    }
}
