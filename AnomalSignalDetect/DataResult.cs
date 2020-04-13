using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AnomalSignalDetect
{
    class DataResult
    {
        public double UpperMargin { get; set; }
        public double LowerMargin { get; set; }

        public double ExpedtedValue { get; set; }
        public int IsAbnormal { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
