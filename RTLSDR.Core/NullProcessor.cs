﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RTLSDR.Core
{
    public class NullProcessor<T> : PipelineBase<T, object>
    {
        public NullProcessor():base(nameof(NullProcessor<T>))
        {

        }

        protected override void doWork(T source)
        {
        }
    }
}
