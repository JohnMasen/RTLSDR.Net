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
        public bool ConsoleOutput { get; set; } = false;
        public bool AutoFlush { get; set; } = false;

        private bool isByte = typeof(T) == typeof(byte);
        private bool isByteArray = typeof(T) == typeof(byte[]);
        private Func<string> headerFunc;
        private StreamWriter writer;
        public SaveToFilePipeline(string fileName, Func<T, string> parser, string header) : base(nameof(SaveToFilePipeline<T>))
        {
            FileName = fileName;
            p = parser;
            if (header != null)
            {
                headerFunc = () => { return header; };
            }
        }

        public SaveToFilePipeline(string fileName, Func<T, string> parser, Func<string> header = null) : base(nameof(SaveToFilePipeline<T>))
        {
            FileName = fileName;
            p = parser;
            headerFunc = header;
        }
        public override void Start(IEnumerable<T> source, CancellationToken token)
        {
            writer = new StreamWriter(FileName, false);
            if (headerFunc != null)
            {
                writer.WriteLine(headerFunc());
            }
            base.Start(source, token);

        }
        protected override void CleanUp()
        {
            writer.Close();
        }
        protected override void doWork(T item)
        {

            if (isByteArray && p==null)
            {
                var s = ((object)item as byte[]).AsSpan<byte>();

                writer.BaseStream.Write(s);
                return;
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
