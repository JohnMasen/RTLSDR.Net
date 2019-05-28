using RTLSDR.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ConsoleTest
{
    class ResultToConsole<T> : PipelineBase<T, object>
    {
        public ResultToConsole():base(nameof(ResultToConsole<string>))
        {

        }
        protected override void doWork(IEnumerable<T> source, CancellationToken token)
        {
            foreach (var item in source)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                Console.WriteLine(item);
            }
        }
    }
}
