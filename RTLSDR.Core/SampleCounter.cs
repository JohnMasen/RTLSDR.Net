using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RTLSDR.Core
{
    public class SampleCounter : PipelineBase<int, Tuple<string, int>>
    {
        int lastValue = 0;
        int counter = 0;
        public SampleCounter() : base(nameof(SampleCounter))
        {

        }
        protected override void doWork(int item)
        {
            


            if (item == lastValue)
            {
                counter++;
            }
            else
            {
                Result.Add(new Tuple<string, int>(lastValue.ToString(), counter));
                counter = 1;
                lastValue = item;
            }

        }
    }
}
