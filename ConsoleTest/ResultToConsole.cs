using RTLSDR.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ConsoleTest
{
    class ResultToConsole<T> : PipelineBase<T, T>
    {
        public ResultToConsole():base(nameof(ResultToConsole<string>))
        {

        }
        protected override void doWork(T item)
        {
            Console.WriteLine(item);
            Result.Add(item);
            
        }
    }
}
