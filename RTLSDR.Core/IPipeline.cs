using System;
using System.Threading;

namespace RTLSDR.Core
{
    public interface IPieline
    {
        int Length { get; }
        string Name { get; }
        WaitHandle WaitHandle { get;  }
    }
}
