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
        Queue<float> buffer;
        public MoveAverage(int size = 10) : base(nameof(MoveAverage))
        {
            this.size = size;
            buffer = new Queue<float>(size + 1);
        }
        protected override void doWork(float item)
        {
            

            buffer.Enqueue(item);
            if (buffer.Count > size)
            {
                buffer.Dequeue();
                Result.Add(buffer.Average());
            }
        }
    }
}
