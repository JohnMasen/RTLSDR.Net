using RTLSDR.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RTLSDR.Core
{
    public abstract class PipelineBase<TSource, TOutput> : IPieline
    {
        public WaitHandle WaitHandle { get; private set; } = new ManualResetEvent(false);
        public string Name { get; set; }
        private CancellationToken cancelToken;
        public PipelineBase(string name)
        {
            Name = name;
            PipelineManager.Default.Register(this);
        }
        public BlockingCollection<TOutput> Result { get; private set; } = new BlockingCollection<TOutput>();
        public void Start(IEnumerable<TSource> source, CancellationToken token)
        {
            cancelToken = token;
            Task.Factory.StartNew(() => 
            {
                doWork(source, token);
                (WaitHandle as ManualResetEvent).Set();
                Console.WriteLine($"{Name} exit success");
            }, token);
        }

        public PipelineBase<TOutput, TClassOutput> Chain<TClassOutput>(PipelineBase<TOutput, TClassOutput> instance)
        {
            instance.Start(Result.GetConsumingEnumerable(), cancelToken);
            return instance;
        }


        protected abstract void doWork(IEnumerable<TSource> source, CancellationToken token);
        public int Length
        {
            get
            {
                return Result.Count;
            }
        }
    }
}
