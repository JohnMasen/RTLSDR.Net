using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RTLSDR.Core
{
    public class IQ2Wave : PipelineBase<Complex, float>
    {
        float radisInSample;
        public IQ2Wave(int Frequency, int Samplerate) : base(nameof(IQ2Wave))
        {
            radisInSample = (float)Math.PI * 2 * Frequency / Samplerate;
        }

        protected override void doWork(Complex item)
        {

            Result.Add(item.Image * (float)Math.Cos(radisInSample) - item.Real * (float)Math.Sin(radisInSample++));
        }
    }
}
