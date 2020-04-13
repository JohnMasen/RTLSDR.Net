using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Text.Json;
using System.Linq;
using Microsoft.Azure.CognitiveServices.AnomalyDetector;
using Microsoft.Azure.CognitiveServices.AnomalyDetector.Models;
using System;
using System.Collections.Generic;

namespace AnomalSignalDetect
{
    public static class MainFunctions
    {
        private static HttpClient client = new HttpClient();

        [FunctionName("AnormalDetect")]
        [return: EventHub("johnhomeeh/anormalydetectevents", Connection = "anomaly-detect-result")]
        public static async Task<string> Run(
            [IoTHubTrigger("messages/events", Connection = "iothub", ConsumerGroup = "anomaly-detect")]EventData message
            , [DurableClient] IDurableEntityClient client
            , ILogger log
            )
        {
            string bodyString = Encoding.UTF8.GetString(message.Body);
            var deviceMessage = JsonSerializer.Deserialize<DeviceRequestBody>(bodyString);

            EntityId id = new EntityId(nameof(DataSeriesEntity), "Default");
            await client.SignalEntityAsync<IDataSeriesEntity>(id, x => x.Enqueue((message.SystemProperties.EnqueuedTimeUtc, deviceMessage.Temperature)));
            var entity = await client.ReadEntityStateAsync<DataSeriesEntity>(id);
            var value = await entity.EntityState.GetValue();

            var valueList = string.Join(",", (from item in value
                                              select item.Value));
            //var data = from item in value
            //           orderby item.date ascending
            //           group item by item.date into itemGroups
            //           select new Point(itemGroups.Key, itemGroups.First().Value);
            var detectResult = await DetectAnomaly(value);
            log.LogInformation($"Total items ={value.Count}");
            if (detectResult.IsAnomaly)
            {
                log.LogWarning("Anomaly detected");
            }
            log.LogInformation($"C# IoT Hub trigger function processed a message: {Encoding.UTF8.GetString(message.Body.Array)}");

            DataResult tmp = new DataResult()
            {
                ExpedtedValue = detectResult.ExpectedValue,
                UpperMargin = detectResult.UpperMargin,
                LowerMargin = detectResult.LowerMargin,
                IsAbnormal = detectResult.IsAnomaly ? 1 : 0,
                Timestamp = message.SystemProperties.EnqueuedTimeUtc
            };
            return JsonSerializer.Serialize(tmp);
            //byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(tmp));

            //EventData result = new EventData(buffer);
            ////result.SystemProperties.Add("iothub-connection-device-id", "AbnormalDetection");
            //return result;
        }

        static IAnomalyDetectorClient createClient()
        {
            string endpoint = GetEnvironmentVariable("AnormalDetecEndpoint");
            string key = GetEnvironmentVariable("AnormalDetecKey");
            IAnomalyDetectorClient client = new AnomalyDetectorClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };
            return client;
        }
        static string GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }

        static async Task<LastDetectResponse> DetectAnomaly(IEnumerable<(DateTime date, double value)> data)
        {
            var client = createClient();
            Request r = new Request();
            r.Series = fixDataSeries(data).ToList();
            var d = JsonSerializer.Serialize(r.Series);
            r.Granularity = Granularity.Minutely;
            try
            {
                return await client.LastDetectAsync(r);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        static IEnumerable<Point> fixDataSeries(IEnumerable<(DateTime date, double value)> data)
        {
            var list = from item in data
                       orderby item.date
                       group item by item.date into itemGroup
                       select new { Date = itemGroup.Key, Value = itemGroup.First().value };
            DateTime now = DateTime.Now;
            TimeSpan t = TimeSpan.Zero;
            foreach (var item in list)
            {
                t = t.Add(TimeSpan.FromMinutes(1));
                yield return new Point(now + t, item.Value);
            }
        }
    }
}