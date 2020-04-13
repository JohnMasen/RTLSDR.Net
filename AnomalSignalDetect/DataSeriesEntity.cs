using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AnomalSignalDetect
{
    public interface IDataSeriesEntity
    {
        void Enqueue((DateTime date, double value) data);
        Task<Queue<(DateTime date, double Value)>> GetValue();
    }
    [JsonObject(MemberSerialization.OptIn)]
    class DataSeriesEntity : IDataSeriesEntity
    {
        private const int MaxItems = 100;
        [JsonProperty("List")]
        public Queue<(DateTime date, double Value)> ValueList { get; set; } = new Queue<(DateTime date, double Value)>();
        public void Enqueue((DateTime date, double value) data)
        {
            while (ValueList.Count>=MaxItems)
            {
                ValueList.Dequeue();
            }
            ValueList.Enqueue((data.date, data.value));
        }

        public Task<Queue<(DateTime date, double Value)>> GetValue() => Task.FromResult(ValueList);

        [FunctionName(nameof(DataSeriesEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext  ctx)
        {
            return ctx.DispatchAsync<DataSeriesEntity>();
        }
    }
}
