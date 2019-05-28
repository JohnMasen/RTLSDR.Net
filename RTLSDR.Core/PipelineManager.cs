using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;

namespace RTLSDR.Core
{
    public class PipelineManager
    {
        public List<IPieline> Pipelines  { get; }= new List<IPieline>();
        public static PipelineManager Default = new PipelineManager();
        private PipelineManager()
        {

        }

        public void Clear()
        {
            Pipelines.Clear();
        }
        public void Register(IPieline instance)
        {
            Pipelines.Add(instance);
        }
        public void WaitAllExit()
        {
            var handles = (from item in Pipelines
                           select item.WaitHandle).ToArray();
            WaitHandle.WaitAll(handles);
        }
    }
}
