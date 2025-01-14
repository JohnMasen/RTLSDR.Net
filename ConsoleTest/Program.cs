﻿using RTLSDR.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace ConsoleTest
{
    class Program
    {
        //天线要放好！！不能放在金属后面！！！
        private const string outputFolder = @"..\..\..\..\SampleData\";
        private const int SampleRate = 250000;
        private const int Frequency = 433920000;

        static RadioConfig[] server = { new RadioConfig()
            {
                DirectSampling=false,
                Frequency=Frequency,
                SampleRate=SampleRate,
                FrequencyCorrection=0,
                Gain=0,
                //ServerIP="127.0.0.1",
                ServerIP="192.168.0.133",
                ServerPort=1234,
                BiasTee=SampleRate
            }
        };

        private const int SIGNAL_LENGTH = 100;
        private static readonly Dictionary<int, byte> map = new Dictionary<int, byte>();


        static void Main(string[] args)
        {
            map.Add(0, 0);
            map.Add(1, 1);

            System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
            //signalDetect(outputFolder + "signal.csv", cts);
            //BitSignalDetect(outputFolder + "signal.csv", cts);
            //MorseSignalDetect("morse.csv", cts);
            //simpleTest(cts);
            WeatherDataOutput("WeatherData.csv", cts);
            //dumpSignalIQ(outputFolder + "IQ.csv", cts);
            //dumpSignalIQRAW(outputFolder + "IQRaw.bin", cts);
            //downSameTest("SingalIQ.csv", "SingalIQ_Down1000.csv",cts);
            //dumpIQToWaveFile(outputFolder+"IQ.csv", outputFolder+"Wave.csv", cts);
            //dumpIQToLPFWaveFile(outputFolder + "IQ.csv", outputFolder + "LPF.csv", cts);
            //dumpIQToLPFWaveSQFile(outputFolder + "IQ.csv", outputFolder + "LPFSQ.csv", 0.6f,cts);
            //dumpIQToLPF_Wave_MA_SQFile(outputFolder + "IQ.csv", outputFolder + "LPF_MV_SQ.csv", 0.3f, cts);
            //dumpIQToLPF_Wave_MA_SQ_SCFile(outputFolder + "IQ.csv", outputFolder + "LPF_MV_SQ_SC.csv", 0.3f, cts);
            dumpQueue(cts.Token);
            Console.ReadLine();
            cts.Cancel();
            Console.WriteLine("waiting for threads exit...");
            PipelineManager.Default.WaitAllExit();
            Console.WriteLine("done");
        }

        private static void simpleTest(CancellationTokenSource cts)
        {
            TCPReader reader = new TCPReader();
            reader
                .Chain(new SignalPreProcessor() { IQOutput = IQOutputEnum.IChannel })
                .Chain(new IQ2Wave(Frequency, SampleRate))
                //.Chain(new LPF(Frequency, SampleRate, 1000f))
                //.Chain(new MoveAverage())
                //.Chain(new SignalCompare(0.2f))
                //.Chain(new SampleCounter())
                //.Chain(new SignalToByteArray(map, SIGNAL_LENGTH))
                //.Chain(new MorseDecode())
                //.Chain(new SignalReverse())
                //.Chain(new MisolWeatherStationDecoder())

                .Chain(new NullProcessor<float>());
            reader.Start(server, cts.Token);
        }

        private static void signalDetect(string outputFileName, CancellationTokenSource cts)
        {
            TCPReader reader = new TCPReader();
            reader.Chain(new SignalPreProcessor() { IQOutput = IQOutputEnum.IChannel })
                .Chain(new IQ2Wave(Frequency, SampleRate))
                //.Chain(new SkipSample<float>(250000 / 1000))
                .Chain(new LPF(Frequency, SampleRate, 1000f))
                .Chain(new MoveAverage())
                .Chain(new SignalCompare(0.2f))
                .Chain(new SampleCounter())
                //.Chain(new SignalToByteArray(map,SIGNAL_LENGTH))
                .Chain(new SaveToFilePipeline<Tuple<int?, int>>(outputFileName, x => { return $"{x.Item1 ?? -1},{x.Item2}"; }) { ConsoleOutput = true });
            //.Chain(new SaveToFilePipeline<float>("output.csv", x => { return $"{x}"; }));
            ;
            reader.Start(server, cts.Token);
        }

        private static void BitSignalDetect(string outputFileName, CancellationTokenSource cts)
        {
            TCPReader reader = new TCPReader();
            reader.Chain(new SignalPreProcessor() { IQOutput = IQOutputEnum.IChannel })
                .Chain(new IQ2Wave(Frequency, SampleRate))
                //.Chain(new SkipSample<float>(250000 / 1000))
                .Chain(new LPF(Frequency, SampleRate, 1000f))
                .Chain(new MoveAverage())
                .Chain(new SignalCompare(0.2f))
                .Chain(new SampleCounter())
                .Chain(new SignalToByteArray(map, SIGNAL_LENGTH))
                .Chain(new SaveToFilePipeline<byte[]>(outputFileName, x => { return $"{BitConverter.ToString(x)}"; }) { ConsoleOutput = true });
            ;
            reader.Start(server, cts.Token);
        }

        private static void WeatherDataOutput(string outputFileName, CancellationTokenSource cts)
        {
            TCPReader reader = new TCPReader();
            reader.Chain(new SignalPreProcessor() { IQOutput = IQOutputEnum.IChannel })
                .Chain(new IQ2Wave(Frequency, SampleRate))
                //.Chain(new SkipSample<float>(250000 / 1000))
                .Chain(new LPF(Frequency, SampleRate, 1000f))
                .Chain(new MoveAverage())
                .Chain(new SignalCompare(0.2f))
                .Chain(new SampleCounter())
                .Chain(new SignalToByteArray(map, SIGNAL_LENGTH))
                .Chain(new MorseDecode())
                .Chain(new SignalReverse())
                .Chain(new MisolWeatherStationDecoder())
                //.Chain(new ResultToConsole<MisolWeahterData>())
                .Chain(new SaveToFilePipeline<MisolWeahterData>(outputFileName, x =>
                {
                    return $"{DateTime.Now.ToString("s")},{Newtonsoft.Json.JsonConvert.SerializeObject(x)}";
                }, "time,data")
                { ConsoleOutput = true });
            ;
            reader.Start(server, cts.Token);
        }

        private static void MorseSignalDetect(string outputFileName, CancellationTokenSource cts)
        {
            TCPReader reader = new TCPReader();
            reader.Chain(new SignalPreProcessor() { IQOutput = IQOutputEnum.IChannel })
                .Chain(new IQ2Wave(Frequency, SampleRate))
                //.Chain(new SkipSample<float>(250000 / 1000))
                .Chain(new LPF(Frequency, SampleRate, 1000f))
                .Chain(new MoveAverage())
                .Chain(new SignalCompare(0.2f))
                .Chain(new SampleCounter())
                .Chain(new SignalToByteArray(map, SIGNAL_LENGTH))
                .Chain(new MorseDecode())
                .Chain(new SignalReverse())
                .Chain(new SaveToFilePipeline<IEnumerable<bool>>(outputFileName, x =>
                {
                    var data = from item in x
                               select item ? "1" : "0";
                    return DateTime.Now.ToString("s") +","+ string.Join(',', data);
                },"time,data")
                { ConsoleOutput = true });
            ;
            reader.Start(server, cts.Token);
        }
        private static void dumpSignal(CancellationTokenSource cts)
        {
            TCPReader reader = new TCPReader();
            reader.Chain(new SignalPreProcessor() { IQOutput = IQOutputEnum.IChannel })
                .Chain(new IQ2Wave(Frequency, SampleRate))
                .Chain(new LPF(Frequency, SampleRate, 1000f))
                .Chain(new SkipSample<float>(SampleRate / 10000))
                //.Chain(new MoveAverage())
                .Chain(new SaveToFilePipeline<float>("output.csv", x => { return $"{x}"; }));
            reader.Start(server, cts.Token);
        }

        private static void dumpSignalIQ(string output, CancellationTokenSource cts)
        {
            TCPReader reader = new TCPReader();
            reader.Chain(new SignalPreProcessor())
                //.Chain(new IQ2Wave(433900000, 250000))
                //.Chain(new LPF(433900000f, 250000f, 1000f))
                //.Chain(new SkipSample<Complex>(100))
                //.Chain(new MoveAverage())
                .Chain(new SaveToFilePipeline<Complex>(output, x => { return $"{x.Image},{x.Real}"; }, "I,Q"));
            reader.Start(server, cts.Token);
        }

        private static void dumpSignalIQRAW(string output, CancellationTokenSource cts)
        {
            TCPReader reader = new TCPReader();
            reader
                //.Chain(new SignalPreProcessor())
                //.Chain(new IQ2Wave(Frequency, 250000))
                //.Chain(new LPF(Frequency, 250000f, 1000f))
                //.Chain(new SkipSample<Complex>(100))
                //.Chain(new MoveAverage())
                .Chain(new SaveToFilePipeline<byte[]>(output, x => { return $"{x}"; }));
            reader.Start(server, cts.Token);
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
        private static void downSameTest(string input, string output, CancellationTokenSource cts)
        {
            var loader = new IQFromCSV();
            loader.Chain(new SkipSample<Complex>(1000))
                .Chain(new SaveToFilePipeline<Complex>(output, x => { return $"{x.Image},{x.Real}"; }, "I,Q") { AutoFlush = true })
                ;
            loader.Start(new string[] { input }, cts.Token);
            Console.WriteLine("done");
        }
        private static void dumpIQToWaveFile(string input, string output, CancellationTokenSource cts)
        {
            var loader = new IQFromCSV();
            loader.Chain(new IQ2Wave(Frequency, SampleRate))
                .Chain(new SaveToFilePipeline<float>(output, x => { return $"{x}"; }, "data") { AutoFlush = true })
                ;
            loader.Start(new string[] { input }, cts.Token);
            Console.WriteLine("done");
        }

        private static void dumpIQToLPFWaveFile(string input, string output, CancellationTokenSource cts)
        {
            var loader = new IQFromCSV();
            loader.Chain(new IQ2Wave(Frequency, SampleRate))
                .Chain(new LPF(Frequency, SampleRate, 1000))
                .Chain(new SaveToFilePipeline<float>(output, x => { return $"{x}"; }, "data") { AutoFlush = true })
                ;
            loader.Start(new string[] { input }, cts.Token);
            Console.WriteLine("done");
        }

        private static void dumpIQToLPFWaveSQFile(string input, string output, float threshhold, CancellationTokenSource cts)
        {
            var loader = new IQFromCSV();
            loader.Chain(new IQ2Wave(Frequency, SampleRate))
                .Chain(new LPF(Frequency, SampleRate, 1000))
                .Chain(new SignalCompare(threshhold))
                .Chain(new SaveToFilePipeline<int>(output, x => { return $"{x}"; }, "data") { AutoFlush = false })
                ;
            loader.Start(new string[] { input }, cts.Token);
            Console.WriteLine("done");
        }
        private static void dumpIQToLPF_Wave_MA_SQFile(string input, string output, float threshhold, CancellationTokenSource cts)
        {
            var loader = new IQFromCSV();
            loader.Chain(new IQ2Wave(Frequency, SampleRate))
                .Chain(new LPF(Frequency, SampleRate, 1000))
                .Chain(new MoveAverage())
                .Chain(new SignalCompare(threshhold))
                .Chain(new SaveToFilePipeline<int>(output, x => { return $"{x}"; }, "data") { AutoFlush = false })
                ;
            loader.Start(new string[] { input }, cts.Token);
        }

        private static void dumpIQToLPF_Wave_MA_SQ_SCFile(string input, string output, float threshhold, CancellationTokenSource cts)
        {
            var loader = new IQFromCSV();
            loader.Chain(new IQ2Wave(Frequency, SampleRate))
                .Chain(new LPF(Frequency, SampleRate, 1000))
                .Chain(new MoveAverage())
                .Chain(new SignalCompare(threshhold))
                .Chain(new SampleCounter())
                .Chain(new SaveToFilePipeline<Tuple<int?, int>>(output, x => { return $"{x.Item1},{x.Item2}"; }, "value,count") { AutoFlush = false })
                ;
            loader.Start(new string[] { input }, cts.Token);
        }
    }
}
