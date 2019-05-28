using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace ConsoleTest
{
    class StreamPipe
    {
        private List<byte[]> buffer = new List<byte[]>();
        private int currentIndex = 0;
        private Memory<byte> input;
        private Memory<byte> output;
        private AutoResetEvent are = new AutoResetEvent(true);
        public int BufferSize { get; private set; }

        
        public StreamPipe(int bufferSize)
        {
            BufferSize = bufferSize;
            input = new byte[BufferSize].AsMemory();
            output = new byte[BufferSize].AsMemory();
            currentIndex = 0;
        }

        private void swapBuffer()
        {
            are.WaitOne();
            var tmp = input;
            input = output;

        }

        public void PushData(byte[] data)
        {

        }

       
    }
}
