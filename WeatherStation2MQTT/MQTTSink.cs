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

    }
    class MQTTSink : PipelineBase<MisolWeahterData, object>
    {
        MqttClient client;
        MQTTConfig config;
        protected override void Init()
        {
            base.Init();
            if (!config.MessagePath.EndsWith("/"))
            {
                config.MessagePath += "/";
            }
            client = new MqttClient(config.BrokerAddress, config.BrokerPort, false, MqttSslProtocols.None, null, null);
            client.Connect("MQTTSink");
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
            string path = $"{config.MessagePath}{source.DeviceID}";
            byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(source));
            client.Publish(path, buffer);
        }

        
    }
}
