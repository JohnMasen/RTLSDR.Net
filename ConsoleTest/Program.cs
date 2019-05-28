using RTLSDR.Core;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            TCPReader reader = new TCPReader("192.168.0.133", 1234);
            System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
            //signalDetect(reader,cts);
            dumpSignalIQ(reader, cts);
            //downSameTest("SingalIQ.csv", "SingalIQ_Down1000.csv",cts);
            //dumpIQToFile("SingalIQ.csv", "SignalWave.csv", cts);
            dumpQueue(cts.Token);
            Console.ReadLine();
            cts.Cancel();
            Console.WriteLine("waiting for threads exit...");
            PipelineManager.Default.WaitAllExit();
            Console.WriteLine("done");
        }
        private static void signalDetect(TCPReader reader,CancellationTokenSource cts)
        {
            reader.Chain(new SignalPreProcessor())
                .Chain(new IQ2Wave(433900000, 250000))
                .Chain(new SkipSample<float>(250000/1000))
                .Chain(new LPF(433900000f, 1000f,1000f))
                .Chain(new SignalCompare(0.06f))
                .Chain(new SampleCounter())
                .Chain(new SaveToFilePipeline<Tuple<string, int>>("output.csv", x => { return $"{x.Item1},{x.Item2}"; }) { ConsoleOutput = true,AutoFlush=true });
            //.Chain(new SaveToFilePipeline<float>("output.csv", x => { return $"{x}"; }));
            ;
            reader.Start(null, cts.Token);
        }
        private static void dumpSignal(TCPReader reader, CancellationTokenSource cts)
        {
            reader.Chain(new SignalPreProcessor())
                .Chain(new IQ2Wave(433900000, 250000))
                .Chain(new LPF(433900000f, 250000f,1000f))
                .Chain(new SkipSample<float>(250000 / 10000))
                //.Chain(new MoveAverage())
                .Chain(new SaveToFilePipeline<float>("output.csv", x => { return $"{x}"; }));
            reader.Start(null,cts.Token);
        }

        private static void dumpSignalIQ(TCPReader reader, CancellationTokenSource cts)
        {
            reader.Chain(new SignalPreProcessor())
                //.Chain(new IQ2Wave(433900000, 250000))
                //.Chain(new LPF(433900000f, 250000f, 1000f))
                .Chain(new SkipSample<Complex>(100))
                //.Chain(new MoveAverage())
                .Chain(new SaveToFilePipeline<Complex>("output.csv", x => { return $"{x.Image},{x.Real}"; },"I,Q"));
            reader.Start(null, cts.Token);
        }
        private static async void dumpQueue(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                foreach (var item in PipelineManager.Default.Pipelines)
                {
                    Console.Write($"{item.Name},{item.Length}     ");
                }
                Console.WriteLine();
                await Task.Delay(1000);
            }
        }
        private static void downSameTest(string input,string output, CancellationTokenSource cts)
        {
            var loader = new IQFromCSV(input);
            loader.Chain(new SkipSample<Complex>(1000))
                .Chain(new SaveToFilePipeline<Complex>(output, x => { return $"{x.Image},{x.Real}"; }, "I,Q") { AutoFlush = true, AddTime=false })
                ;
            loader.Start(null,cts.Token);
            Console.WriteLine("done");
        }
        private static void dumpIQToFile(string input, string output, CancellationTokenSource cts)
        {
            var loader = new IQFromCSV(input);
            loader.Chain(new IQ2Wave(433900000,250000))
                .Chain(new SaveToFilePipeline<float>(output, x => { return $"{x}"; }, "data") { AutoFlush = true, AddTime = false })
                ;
            loader.Start(null, cts.Token);
            Console.WriteLine("done");
        }
    }
}
