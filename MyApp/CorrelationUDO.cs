
namespace MyApp
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Reactive;
    using System.Reflection;
    using Microsoft.ComplexEventProcessing;
    using Microsoft.ComplexEventProcessing.Linq;
    using System.Collections.Generic;
    using Microsoft.ComplexEventProcessing.Extensibility;
    using StreamInsight.Samples.Adapters.AsyncCsv;
    using System.Diagnostics;


    public class CorrelationUDO : CepAggregate<MyReading, string>
    {
        public override string GenerateOutput(IEnumerable<MyReading> Stream)
        {
            List<double> items = new List<double>();
            List<double> items2 = new List<double>();
            List<double> items3 = new List<double>();
            List<double> items4 = new List<double>();

            foreach (var e in Stream) 
            { 
                 items.Add(double.Parse(e.input2, System.Globalization.CultureInfo.InvariantCulture));
                items2.Add(double.Parse(e.input3, System.Globalization.CultureInfo.InvariantCulture));
                items3.Add(double.Parse(e.input4, System.Globalization.CultureInfo.InvariantCulture));
                items4.Add(double.Parse(e.input5, System.Globalization.CultureInfo.InvariantCulture)); 
            }

            double[] correl = new double[4];
            double[] array1 = items.ToArray();
            double[] array2 = items2.ToArray();
            double[] array3 = items3.ToArray();
            double[] array4 = items4.ToArray();

            correl[0] = CorrelationFactor.GetCorrelation(array1, array1);
            correl[1] = CorrelationFactor.GetCorrelation(array1, array2);
            correl[2] = CorrelationFactor.GetCorrelation(array1, array3);
            correl[3] = CorrelationFactor.GetCorrelation(array1, array4);

            string correlations;
            correlations = ',' + correl[0].ToString(CultureInfo.InvariantCulture.NumberFormat)
                + ',' + correl[1].ToString(CultureInfo.InvariantCulture.NumberFormat)
                + ',' + correl[2].ToString(CultureInfo.InvariantCulture.NumberFormat)
                + ',' + correl[3].ToString(CultureInfo.InvariantCulture.NumberFormat);
            return correlations;

        }
    }
}