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
        float value = 0f;
        float weight;
        public MoveAverage(int size = 10) : base(nameof(MoveAverage))
        {
            this.size = size;
            buffer = new Queue<float>(size);
            weight = 1f / size;
        }
        protected override void doWork(float item)
        {
            float temp= item * weight;

            buffer.Enqueue(temp);
            value += temp;

            if (buffer.Count == size)
            {
                Result.Add(value);
                value -=buffer.Dequeue();
            }
        }
    }
}
