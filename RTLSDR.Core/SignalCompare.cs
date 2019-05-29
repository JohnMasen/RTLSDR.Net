using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RTLSDR.Core
{
    public class SignalCompare : PipelineBase<float, int>
    {
        private float t;
        public SignalCompare(float threshold) : base(nameof(SignalCompare))
        {
            t = threshold;
        }
        protected override void doWork(float item)
        {


            Result.Add(item >= t?1:0);

        }
    }
}
