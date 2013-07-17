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
    public class CorrelationFactor
    {
        public static double GetAverage(double[] data)
        {
            int len = data.Length;

            if (len == 0)
                throw new Exception("No data");

            double sum = 0;

            for (int i = 0; i < data.Length; i++)
                sum += data[i];

            return sum / len;
        }

        public static double GetVariance(double[] data)
        {
            int len = data.Length;

            // Get average
            double avg = GetAverage(data);

            double sum = 0;

            for (int i = 0; i < data.Length; i++)
                sum += System.Math.Pow((data[i] - avg), 2);

            return sum / len;
        }
        public static double GetStdev(double[] data)
        {
            return Math.Sqrt(GetVariance(data));
        }

        public static double GetCorrelation(double[] x, double[] y)
        {
            if (x.Length != y.Length)
                throw new Exception("Length of sources is different");
            double avgX = GetAverage(x);
            double stdevX = GetStdev(x);
            double avgY = GetAverage(y);
            double stdevY = GetStdev(y);
            double covXY = 0;
            double pearson = 0;
            int len = x.Length;
            for (int i = 0; i < len; i++)
                covXY += (x[i] - avgX) * (y[i] - avgY);
            covXY /= len;
            pearson = covXY / (stdevX * stdevY);
            return pearson;
        }
    }
}
