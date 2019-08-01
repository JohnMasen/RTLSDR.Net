using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RTLSDR.Core
{
    public class LPF : PipelineBase<float, float>
    {
        private float a;
        public LPF(float frequency, float samplerate, float cutoff) : base(nameof(LPF))
        {
            a = 1f / (1f + 1f / (2f * (float)Math.PI * (frequency / samplerate) * cutoff));
        }
        private float lastValue = 0f;
        protected override void doWork(float item)
        {

            float v = item < 0f ? -item : item;

            float value = lastValue + a * (v - lastValue);
            lastValue = value;
            Result.Add(value);

        }
    }
}
