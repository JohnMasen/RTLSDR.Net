using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RTLSDR.Core
{
    public class IQ2Wave : PipelineBase<Complex, float>
    {
        float radisInSample;
        float[] SinRadisInSample, CosRadisInSample;
        const float scale = 1.0f / 127.5f;
        public IQ2Wave(int Frequency, int Samplerate) : base(nameof(IQ2Wave))
        {

            SinRadisInSample = new float[256];
            CosRadisInSample = new float[256];
            radisInSample = (float)Math.PI * 2 * Frequency / Samplerate;
            for (int i = 0; i < 256; i++)
            {
                float value = (i - 127.5f) * scale;
                SinRadisInSample[i] = (float)Math.Sin(radisInSample)*value;
                CosRadisInSample[i] = (float)Math.Cos(radisInSample)*value;
            }
        }

        protected override void doWork(Complex item)
        {

            Result.Add(CosRadisInSample[item.Image] - SinRadisInSample[item.Real]);
        }
    }
}
