using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace RTLSDR.Core
{
    public class SaveToFilePipeline<T> : PipelineBase<T, object>
    {
        public string FileName { get; private set; }
        private Func<T, string> p;
        public bool AddTime { get; set; } = true;
        public bool ConsoleOutput { get; set; } = false;
        public bool AutoFlush { get; set; } = false;
        private Func<string> headerFunc;
        public SaveToFilePipeline(string fileName, Func<T, string> parser, string header) :base(nameof(SaveToFilePipeline<T>))
        {
            FileName = fileName;
            p = parser;
            if (header!=null)
            {
                headerFunc = () => { return header; };
            }
        }
        
        public SaveToFilePipeline(string fileName, Func<T, string> parser, Func<string> header=null) : base(nameof(SaveToFilePipeline<T>))
        {
            FileName = fileName;
            p = parser;
            headerFunc = header;
        }
        protected override void doWork(IEnumerable<T> source, CancellationToken token)
        {
            Stopwatch sw = new Stopwatch();

            using (StreamWriter writer = new StreamWriter(FileName, false))
            {
                if (headerFunc != null)
                {
                    if (AddTime)
                    {
                        writer.Write("TimeStamp,");
                    }
                    writer.WriteLine(headerFunc());
                }
                sw.Start();
                foreach (var item in source)
                {
                    if (AddTime)
                    {
                        writer.Write(sw.ElapsedTicks);
                        writer.Write(",");
                    }
                    writer.WriteLine(p(item));
                    if (ConsoleOutput)
                    {
                        Console.WriteLine(p(item));
                    }
                    if (AutoFlush)
                    {
                        writer.Flush();
                    }
                }
            }
        }
    }
}
