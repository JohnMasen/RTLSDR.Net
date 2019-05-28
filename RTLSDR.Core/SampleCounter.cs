using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RTLSDR.Core
{
    public class SampleCounter : PipelineBase<bool, Tuple<string, int>>
    {
        public SampleCounter():base(nameof(SampleCounter))
        {

        }
        protected override void doWork(IEnumerable<bool> source, CancellationToken token)
        {
            bool lastValue = false;
            int counter = 0;
            foreach (var item in source)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                if (item==lastValue)
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
}
