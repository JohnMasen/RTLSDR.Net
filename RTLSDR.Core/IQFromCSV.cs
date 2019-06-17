using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace RTLSDR.Core
{
    public class IQFromCSV : PipelineBase<string, Complex>
    {
        public IQFromCSV() : base(nameof(IQFromCSV))
        {
        }
        
        protected override void doWork(string fileName)
        {
            using (StreamReader reader = new StreamReader(fileName))
            {
                string line = reader.ReadLine();
                var index = DetectIndex(line);
                byte i, q;
                line = reader.ReadLine();
                while (line != null)
                {
                    var items = line.Split(",");
                    if (byte.TryParse(items[index.I], out i) && byte.TryParse(items[index.Q], out q))
                    {
                        Result.Add(new Complex() { Image = i, Real = q });
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                    line = reader.ReadLine();
                }

            }
        }
        private (int I, int Q) DetectIndex(string s)
        {
            var tmp = s.Split(",");
            int index_i=-1, index_q=-1;
            for (int i = 0; i < tmp.Length; i++)
            {
                if (string.Compare(tmp[i], "i", true)==0)
                {
                    index_i = i;
                }
                if (string.Compare(tmp[i],"q",true)==0)
                {
                    index_q = i;
                }
            }
            if (index_q==-1 || index_q==-1)
            {
                throw new InvalidOperationException("CSV file does not contain I,Q in column header");
            }
            return (index_i, index_q);
        }
    }
}
