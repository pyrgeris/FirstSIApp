
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


    public class MyApp
    {

        //Print Available Queries Menu to User

        private static int AskUserWhichQuery()
        {
            Console.WriteLine();
            Console.WriteLine("Pick a query:");
            Console.WriteLine("0. Pass-through");
            Console.WriteLine("1. Hop-Count");
            Console.WriteLine("2. Correlation Tumbling Window");
            Console.WriteLine("3. Exit");

            string queryNumStr = Console.ReadLine();
            int queryNum;

            try
            {
                queryNum = Int32.Parse(queryNumStr, new CultureInfo("en-US"));
            }
            catch (FormatException)
            {
                queryNum = 3;
            }

            return queryNum;
        }

        //Queries

        static void PassThrough()
        {
            var inputStream = CepStream<MyReading>.Create("MyStream");
            // Define input stream object, mapped to stream names
            // Instantiate an adapter from the input factory class
            var queryStream = from e in inputStream select e;
            BindandQuery.BindAndRunQuery(
                "PassThrough",
                queryStream,
                EventShape.Interval,
                new List<string>() { "input1", "input2", "input3", "input4" , "input5"},
                new List<string>() { "input1", "input2", "input3", "input4", "input5" });

        }

        static void HopCount()
        {
            var inputStream = CepStream<MyReading>.Create("MyStream");
            // Define input stream object, mapped to stream names
            // Instantiate an adapter from the input factory class
            var queryStream = from w in inputStream.HoppingWindow(TimeSpan.FromMilliseconds(20000),    // Window Size
                                            TimeSpan.FromMilliseconds(10 * 200),    // Hop Size
                                            HoppingWindowOutputPolicy.ClipToWindowEnd)
                              select new HopCount { Count = w.Count() };
            BindandQuery.BindAndRunQuery(
                "HopCount",
                queryStream,
                EventShape.Interval,
                new List<string>() { "input1", "input2", "input3", "input4", "input5" },
                new List<string>() { "Count" });

        }

        static void Correlation_Tumbling_Window()
        {
            var inputStream = CepStream<MyReading>.Create("MyStream");
            // Define input stream object, mapped to stream names
            // Instantiate an adapter from the input factory class
            var countStream = from win in inputStream.TumblingWindow(TimeSpan.FromMilliseconds(20000), HoppingWindowOutputPolicy.ClipToWindowEnd)
                              select new Correlations { Correlation = win.UserDefinedAggregate<MyReading, CorrelationUDO, string>(null) };              

            BindandQuery.BindAndRunQuery(
                "Correlation Tumbling Window",
                countStream,
                EventShape.Point,
                new List<string>() { "input1", "input2", "input3", "input4", "input5" },
                new List<string>() { "Correlation" } );

        }

        //Trivial Main

        internal static void Main()
        {
            while (true)
            {
                int queryToRun = AskUserWhichQuery();

                switch (queryToRun)
                {
                    case 0:
                        PassThrough();
                        break;
                    case 1:
                        HopCount();
                        break;
                    case 2:
                        Correlation_Tumbling_Window();
                        break;
                    case 12:
                        return;
                    default:
                        Console.WriteLine("Unknown Query Demo");
                        break;
                }
            }
        }



    }
}