using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RTLSDR.Core
{
    public class SignalCompare : PipelineBase<float, Boolean>
    {
        private float t;
        public SignalCompare(float threshold):base(nameof(SignalCompare))
        {
            t = threshold;
        }
        protected override void doWork(IEnumerable<float> source, CancellationToken token)
        {
            foreach (var item in source)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                Result.Add(item >= t);
            }
        }
    }
}
