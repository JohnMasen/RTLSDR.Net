using RTLSDR.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RTLSDR.Core
{
    public struct Complex
    {
        public float Real;
        public float Image;
        public override string ToString()
        {
            return $"Real={Real},Image={Image}";
        }
    }
    [Flags]
    public enum IQOutputEnum
    {
        IChannel,
        QChannel
    }

    public class SignalPreProcessor : PipelineBase<byte[], Complex>
    {
        public IQOutputEnum IQOutput { get; set; } = IQOutputEnum.IChannel | IQOutputEnum.QChannel;
        private float[] map = new float[256];
        const float scale = 1.0f / 127.5f;
        public SignalPreProcessor() : base(nameof(SignalPreProcessor))
        {
            for (int i = 0; i < 256; i++)
            {
                map[i] = (i - 127.5f) * scale;
            }
        }

        protected override void doWork(byte[] item)
        {

            if (item.Length % 2 != 0)
            {
                throw new InvalidOperationException("byte size error");
            }
            var itemSpan = item.AsSpan();
            int index = 0;
            while (index < item.Length)
            {
                Complex r = new Complex
                {
                    Image = IQOutput.HasFlag(IQOutputEnum.IChannel) ? map[itemSpan[index++]] : 0f,
                    Real = IQOutput.HasFlag(IQOutputEnum.IChannel) ? map[itemSpan[index++]] : 0f,
                };
                Result.Add(r);
            }


        }
    }
}
