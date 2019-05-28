using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RTLSDR.Core
{
    public class LPF : PipelineBase<float, float>
    {
        private float a;
        public LPF(float frequency,float samplerate,float cutoff):base(nameof(LPF))
        {
            a = 1f / (1f + 1f / (2f * (float)Math.PI *(frequency / samplerate)*cutoff));
        }
        private float lastValue = 0f;
        protected override void doWork(IEnumerable<float> source, CancellationToken token)
        {
            foreach (var item in source)
            {
                float v = Math.Max(item,0);
                if (token.IsCancellationRequested)
                {
                    return;
                }
                float value = lastValue + a * (v - lastValue);
                lastValue = v;
                Result.Add(v);
            }
            
        }
    }
}
