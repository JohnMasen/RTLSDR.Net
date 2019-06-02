using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Linq;

namespace RTLSDR.Core
{
    public enum WindDirectionEnum:byte
    {

    }
    public class MisolWeahterData
    {
        public int DeviceID { get;  set; }
        public float Temperature { get;  set; }
        public int Humidity { get;  set; }
        public float WindSpeed { get;  set; }
        public float GustSpeed { get;  set; }
        public float Rain { get;  set; }
        public bool IsLowBattery { get;  set; }
        public string WindDirection { get;  set; }

    }
    public class MisolWeatherStationDecoder : PipelineBase<IEnumerable<bool>, MisolWeahterData>
    {
        private string[] WindDirections = new string[] { "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW" };
        private Dictionary<string,(int pos, int length)> map = new Dictionary<string,(int pos, int length)>()
        {
            { nameof(MisolWeahterData.DeviceID),(11,8) },
            {nameof(MisolWeahterData.Temperature),(19,12) },
            {nameof(MisolWeahterData.Humidity),(31,8) },
            {nameof(MisolWeahterData.WindSpeed),(39,8) },
            {nameof(MisolWeahterData.GustSpeed),(47,8) },
            {nameof(MisolWeahterData.Rain),(55,12) },
            {nameof(MisolWeahterData.IsLowBattery),(70,1) },
            {nameof(MisolWeahterData.WindDirection),(71,8)}
        };
        public MisolWeatherStationDecoder():base(nameof(MisolWeatherStationDecoder))
        {

        }
        protected override void doWork(IEnumerable<bool> source)
        {
            if (source.Count()!=87)
            {
                return;
            }
            MisolWeahterData data = new MisolWeahterData();
            var reader = source.ToArray().AsSpan();
            data.DeviceID = readByName(reader,nameof(MisolWeahterData.DeviceID));
            data.Temperature = (readByName(reader, nameof(MisolWeahterData.Temperature))-400)/10f;
            data.Humidity = readByName(reader, nameof(MisolWeahterData.Humidity));
            data.WindSpeed = readByName(reader, nameof(MisolWeahterData.WindSpeed)) * 0.34f;
            data.GustSpeed = readByName(reader, nameof(MisolWeahterData.GustSpeed)) * 0.34f;
            data.Rain = readByName(reader, nameof(MisolWeahterData.Rain)) * 0.3f;
            data.IsLowBattery = readByName(reader, nameof(MisolWeahterData.IsLowBattery)) == 1;
            data.WindDirection = WindDirections[readByName(reader, nameof(MisolWeahterData.WindDirection))];
            Result.Add(data);
        }
        private int readByName(Span<bool> data, string name)
        {
            var item = map[name];
            return readFromBit(data.Slice(item.pos, item.length));
        }

        private int readFromBit(ReadOnlySpan<bool> source)
        {
            int result = 0;
            for (int i = 0; i < source.Length; i++)
            {
                result <<= 1;
                result |= source[i] ? 1 : 0;
            }
            return result;
        }
    }
}
