using System;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace RTLSDR.Core
{
    public class SignalReverse : PipelineBase<IEnumerable<bool>, IEnumerable<bool>>
    {
        public SignalReverse():base(nameof(SignalReverse))
        {

        }
        protected override void doWork(IEnumerable<bool> source)
        {
            var tmp = from item in source
                      select !item;
            Result.Add(tmp);
        }
    }
}
