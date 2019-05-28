using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RTLSDR.Core
{
    public class SkipSample<T> : PipelineBase<T, T>
    {
        public int Gap { get; private set; }
        public SkipSample(int gap):base(nameof(SkipSample<T>))
        {
            Gap = gap;
        }
        protected override void doWork(IEnumerable<T> source, CancellationToken token)
        {
            int count = 0;
            foreach (var item in source)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                if (count<Gap)
                {
                    count++;
                }
                else
                {
                    Result.Add(item);
                    count = 0;
                }
            }
        }
    }
}
