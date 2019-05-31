using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RTLSDR.Core
{
    public class SampleCounter : PipelineBase<int, Tuple<int?, int>>
    {
        int lastValue = 0;
        int counter = 0;
        int stopSteps;
        public SampleCounter(int stopSteps=400) : base(nameof(SampleCounter))
        {
            this.stopSteps = stopSteps;
        }
        protected override void doWork(int item)
        {
            if (item == lastValue)
            {
                counter++;
                if (counter==stopSteps)
                {
                    Result.Add(new Tuple<int?, int>(null, 0));
                }
            }
            else
            {
                Result.Add(new Tuple<int?, int>(lastValue, counter));
                counter = 1;
                lastValue = item;
            }

        }
    }
}
