using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace RTLSDR.Core
{
    public class SignalToByteArray : PipelineBase<Tuple<int?, int>, byte[]>
    {
        List<Tuple<int, int>> buffer = new List<Tuple<int, int>>();
        Dictionary<int, byte> map;
        int length;
        public SignalToByteArray(Dictionary<int, byte> valueMap,int signalLength) :base(nameof(SignalToByteArray))
        {
            map = valueMap;
            length = signalLength;
        }
        protected override void doWork(Tuple<int?, int> source)
        {
            if (source.Item1==null) //stop sign detected
            {
                if (buffer.Count==0)//signal counter will return a stop sign at start
                {
                    return;
                }
                buffer.RemoveAt(0);//first value is the length between last signal,
                Result.Add(convertSignal(buffer).ToArray());
                buffer.Clear();
            }
            else
            {
                buffer.Add(new Tuple<int, int>(source.Item1.Value, source.Item2));
            }
        }

        private IEnumerable<byte> convertSignal(List<Tuple<int, int>> data)
        {
            foreach (var value in data)
            {
                byte signal = map[value.Item1];
                for (int i = 0; i < value.Item2 / length; i++)
                {
                    yield return signal;
                }
            }
            
        }
    }
}
