using Microsoft.ComplexEventProcessing.Linq;
using Microsoft.ComplexEventProcessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using System.Threading;

namespace MyApp
{
   
    public class MyReading
    {
        public string input1 { get; set; }
        public string input2 { get; set; }
        public string input3 { get; set; }
        public string input4 { get; set; }
        public string input5 { get; set; }
    }

    public class HopCount
    {
        public double Count { get; set; }
    }

    public class Correlations
    {
        public string Correlation { get; set; }
    }


}
