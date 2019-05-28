using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
namespace RTLSDR.Core
{
    public class MoveAverage : PipelineBase<float, float>
    {
        private int size;
        public MoveAverage(int size=10):base(nameof(MoveAverage))
        {
            this.size = size;
        }
        protected override void doWork(IEnumerable<float> source, CancellationToken token)
        {
            Queue<float> buffer = new Queue<float>();
            foreach (var item in source)
            {
                buffer.Enqueue(item);
                if (buffer.Count>size)
                {
                    buffer.Dequeue();
                    Result.Add(buffer.Average());
                }
                
            }
        }
    }
}
