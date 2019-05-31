using System;
using System.Collections.Generic;
using System.Text;

namespace RTLSDR.Core
{
    public class MorseDecode : PipelineBase<byte[], IEnumerable<bool>>
    {
        public int SignalSequenceLengthMin { get; private set; }
        public int SignalSequenceLengthMax { get; private set; }
        public int EmptySequenceLength { get; private set; }
        private byte[] empty;
        private const byte ONE = 1;
        private const byte ZERO = 0;
        public MorseDecode(int minLength = 1, int maxLength = 3, int emptyLength = 2) : base(nameof(MorseDecode))
        {
            SignalSequenceLengthMin = minLength;
            SignalSequenceLengthMax = maxLength;
            EmptySequenceLength = emptyLength;
            empty = new byte[emptyLength];
        }
        protected override void doWork(byte[] source)
        {
            var data = source.AsSpan();

            int pos = 0;
            List<bool> tmp = new List<bool>();
            bool isDetectEmpty = false;
            while (pos < data.Length)
            {
                var result = detectBit(source.AsSpan(pos), isDetectEmpty);
                if (result.isSuccess && !isDetectEmpty)
                {
                    tmp.Add(result.code.Value);
                }
                pos += result.length;
            }
            if (tmp.Count>0)
            {
                Result.Add(tmp);
            }
        }

        private (bool isSuccess, bool? code, int length) detectBit(ReadOnlySpan<byte> data,bool isEmpty)
        {
            if (isEmpty)
            {
                if (data.StartsWith(empty))
                {
                    return (true,null,empty.Length);
                }
                else
                {
                    return (false,null,1);
                }
            }
            else
            {
                var length=data.IndexOf(ZERO);
                if (length==0) //current byte is ZERO, expect ONE
                {
                    return (false, null, 1);
                }
                if (length==-1)//all following values are ONE
                {
                    length = data.Length;
                }
                if (length<=SignalSequenceLengthMin)
                {
                    return (true, false, length);
                }
                if (length>=SignalSequenceLengthMax)
                {
                    return (true, true, length);
                }
                return (false, null, 1);
            }
        }


    }
}
