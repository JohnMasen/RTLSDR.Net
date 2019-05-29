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
        public virtual void Start(IEnumerable<TSource> source, CancellationToken token)
        {
            cancelToken = token;
            Task.Factory.StartNew(() => 
            {
                Init();
                foreach (var item in source)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                    doWork(item);
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                }
                CleanUp();
                Result.CompleteAdding();
                (WaitHandle as ManualResetEvent).Set();
                Console.WriteLine($"{Name} exit success");
            }, token);
        }
        protected virtual void Init()
        {

        }

        public PipelineBase<TOutput, TClassOutput> Chain<TClassOutput>(PipelineBase<TOutput, TClassOutput> instance)
        {
            instance.Start(Result.GetConsumingEnumerable(), cancelToken);
            return instance;
        }

        protected virtual void CleanUp()
        {

        }

        protected abstract void doWork(TSource source);
        public int Length
        {
            get
            {
                return Result.Count;
            }
        }
    }
}
