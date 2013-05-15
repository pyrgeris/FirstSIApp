
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

    static class MyApp
    {
        [DisplayName("Pass-through")]
        [Description("Pass-through query to just show input stream in the same form as we show output.")]
        static void PassThrough(Application app)
        {

            var inputStream = GetNewSensorStream(app);
            var query1 = from e in inputStream
                         select e;
            app.DisplayIntervalResults(query1);

        }


        [DisplayName("Hopping Count")]
        [Description("Report the count of input values being processed at some time over " +
                     "a 10000 ms window, with the window moving in 200 ms hops. " +
                     "Provide the counts as of the last reported result as of a point " +
                     "in time, reflecting the input values processed over the last 10000 ms.")]
        static void HoppingCount(Application app)
        {
            var inputStream = app.GetNewSensorStream();
            var countStream = from win in inputStream.HoppingWindow(TimeSpan.FromMilliseconds(10000), TimeSpan.FromMilliseconds(200))
                              select win.Count();
            var query2 = countStream.ToPointEventStream();
            app.DisplayPointResults(query2);
        }


        [DisplayName("Correlation Factor")]
        [Description("Report the count of input values being processed at some time over \n" +
                     "a 10000 ms window, with the window moving in 200 ms hops. \n" +
                     "Provide the counts as of the last reported result as of a point \n" +
                     "in time, reflecting the input values processed over the last 10000 ms.\n")]
        static void CorrelationFactor(Application app)
        {
            var inputStream = app.GetNewSensorStream();
            var countStream = from win in inputStream.HoppingWindow(TimeSpan.FromMilliseconds(10000), TimeSpan.FromMilliseconds(200))
                              select win.UserDefinedAggregate<newSensorReading, Correlation, double>(null);
            var query3 = countStream.ToPointEventStream();
            app.DisplayPointResults(query3);

        }

        static IQStreamable<newSensorReading> GetNewSensorStream(this Application app)
        {
            //AdvanceTimeGenerationSettings atgs =  new AdvanceTimeGenerationSettings(1,-TimeSpan.FromMilliseconds(100));
            //AdvanceTimeSettings ats = new AdvanceTimeSettings(atgs,null,AdvanceTimePolicy.Drop);
            IEnumerable<newSensorReading> newsensorReadings = newSensorReading.getSensorReadings(newSensorReadingsFileName);
            var newsensorStream = app.DefineEnumerable<newSensorReading>(() => newsensorReadings).ToPointStreamable<newSensorReading, newSensorReading>(
                        e => PointEvent.CreateInsert<newSensorReading>(e.StartTime, e),
                        AdvanceTimeSettings.IncreasingStartTime);
            return newsensorStream;
        }

        public class Correlation : CepAggregate<newSensorReading, double>
        {
            public override double GenerateOutput(IEnumerable<newSensorReading> newsensorStream)
            {
                List<double> items = new List<double>();
                List<double> items2 = new List<double>();
                foreach (var e in newsensorStream) { items.Add(e.input3); items2.Add(e.input2); }

                double[] array1 = items.ToArray();
                double[] array2 = items2.ToArray();

                double[] array_xy = new double[array1.Length];
                double[] array_xp2 = new double[array1.Length];
                double[] array_yp2 = new double[array1.Length];
                for (int i = 0; i < array1.Length; i++)
                    array_xy[i] = array1[i] * array2[i];
                for (int i = 0; i < array1.Length; i++)
                    array_xp2[i] = Math.Pow(array1[i], 2.0);
                for (int i = 0; i < array1.Length; i++)
                    array_yp2[i] = Math.Pow(array2[i], 2.0);
                double sum_x = 0;
                double sum_y = 0;
                foreach (double n in array1)
                    sum_x += n;
                foreach (double n in array2)
                    sum_y += n;
                double sum_xy = 0;
                foreach (double n in array_xy)
                    sum_xy += n;
                double sum_xpow2 = 0;
                foreach (double n in array_xp2)
                    sum_xpow2 += n;
                double sum_ypow2 = 0;
                foreach (double n in array_yp2)
                    sum_ypow2 += n;
                double Ex2 = Math.Pow(sum_x, 2.00);
                double Ey2 = Math.Pow(sum_y, 2.00);

                double Correl = (array1.Length * sum_xy - sum_x * sum_y) / Math.Sqrt((array1.Length * sum_xpow2 - Ex2) * (array1.Length * sum_ypow2 - Ey2));

                return Correl;

            }

        }


        #region Program plumbing

        static void DisplayPointResults<TPayload>(this Application app, IQStreamable<TPayload> resultStream)
        {
            // Define observer that formats arriving events as points to the console window.
            var consoleObserver = app.DefineObserver(() => Observer.Create<PointEvent<TPayload>>(ConsoleWritePoint));

            // Bind resultStream stream to consoleObserver.
            var binding = resultStream.Bind(consoleObserver);

            // Run example by creating a process from the binding we built above.
            using (binding.Run("ExampleProcess"))
            {
                Console.WriteLine("***Hit Return to exit after viewing query output***");
                Console.WriteLine();
                Console.ReadLine();
            }
        }

        static void ConsoleWritePoint<TPayload>(PointEvent<TPayload> e)
        {
            if (e.EventKind == EventKind.Insert)
                WriteLine(string.Format(CultureInfo.InvariantCulture, "INSERT <{0}> {1}", e.StartTime.DateTime, e.Payload.ToString()));
            else
                WriteLine(string.Format(CultureInfo.InvariantCulture, "CTI    <{0}>", e.StartTime.DateTime));
        }

        static void DisplayIntervalResults<TPayload>(this Application app, IQStreamable<TPayload> resultStream)
        {
            // Define observer that formats arriving events as intervals to the console window.
            var consoleObserver = app.DefineObserver(() => Observer.Create<IntervalEvent<TPayload>>(ConsoleWriteInterval));

            // Bind resultStream stream to consoleObserver.
            var binding = resultStream.Bind(consoleObserver);

            // Run example query by creating a process from the binding we've built above.
            using (binding.Run("ExampleProcess"))
            {
                Console.WriteLine("***Hit Return to exit after viewing query output***");
                Console.WriteLine();
                Console.ReadLine();
            }
        }

        static void ConsoleWriteInterval<TPayload>(IntervalEvent<TPayload> e)
        {
            if (e.EventKind == EventKind.Insert)
                WriteLine(string.Format(CultureInfo.InvariantCulture, "INSERT <{0} - {1}> {2}", e.StartTime.DateTime, e.EndTime.DateTime, e.Payload.ToString()));
            else
                WriteLine(string.Format(CultureInfo.InvariantCulture, "CTI    <{0}>", e.StartTime.DateTime));
        }

        static void DisplayEdgeResults<TPayload>(this Application app, IQStreamable<TPayload> resultStream)
        {
            // Define observer that formats arriving events as intervals to the console window.
            var consoleObserver = app.DefineObserver(() => Observer.Create<EdgeEvent<TPayload>>(ConsoleWriteEdge));

            // Bind resultStream stream to consoleObserver.
            var binding = resultStream.Bind(consoleObserver);

            // Run example query by creating a process from the binding we've built above.
            using (binding.Run("ExampleProcess"))
            {
                Console.WriteLine("***Hit Return to exit after viewing query output***");
                Console.WriteLine();
                Console.ReadLine();
            }
        }

        static void ConsoleWriteEdge<TPayload>(EdgeEvent<TPayload> e)
        {
            if (e.EventKind == EventKind.Insert)
            {
                WriteLine(string.Format(CultureInfo.InvariantCulture, "START <{0} - {1}> {2}", e.StartTime.DateTime, "Infinitive", e.Payload.ToString()));
            }
            else
                WriteLine(string.Format(CultureInfo.InvariantCulture, "CTI    <{0}>", e.StartTime.DateTime));
        }

        private static void WriteLine(String line)
        {
            Console.WriteLine(line);
            Console.WriteLine();//add one more empty line to for clearer display

            writeToOutputFile(line);
        }

        private static void WriteLine()
        {
            WriteLine("");
        }


        private static string newSensorReadingsFileName = @"Input Data\input.csv";

        private static string OutputFileName = @"output.txt";

        private static void prepareOutputFile()
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(OutputFileName);
            file.WriteLine("------RESULT-----");
            file.Close();
        }

        private static void writeToOutputFile(String line)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(OutputFileName, true);
            file.WriteLine(line);
            file.Close();
        }

        static void Main()
        {
            Console.WriteLine("Output data will be displayed on console and also persited to file " + OutputFileName + ".\n");

            using (var server = Server.Create("instance1"))
            {

                Application application = server.CreateApplication("AssignmentApp");

                // collect all supported queries as defined in the above sections of this code
                var demos = (from mi in typeof(MyApp).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                             let nameAttr = mi.GetCustomAttributes(typeof(DisplayNameAttribute), false)
                                 .OfType<DisplayNameAttribute>()
                                 .SingleOrDefault()
                             let descriptionAttr = mi.GetCustomAttributes(typeof(DescriptionAttribute), false)
                                 .OfType<DescriptionAttribute>()
                                 .SingleOrDefault()
                             where null != nameAttr
                             select new { Action = mi, Name = nameAttr.DisplayName, Description = descriptionAttr.Description }).ToArray();

                prepareOutputFile();

                // repeated ask user for a query type to run
                while (true)
                {
                    Console.WriteLine();
                    Console.WriteLine("Pick a query type:");
                    for (int demo = 0; demo < demos.Length; demo++)
                    {
                        Console.WriteLine("{0,4} - {1}", demo, demos[demo].Name);
                    }

                    Console.WriteLine("Exit - Exit from Demo.");
                    var response = Console.ReadLine().Trim();
                    if (string.Equals(response, "exit", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(response, "e", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }

                    int demoToRun;
                    demoToRun = Int32.TryParse(response, NumberStyles.Integer, CultureInfo.InvariantCulture, out demoToRun)
                        ? demoToRun
                        : -1;

                    if (0 <= demoToRun && demoToRun < demos.Length)
                    {
                        WriteLine();
                        WriteLine(demos[demoToRun].Name);
                        WriteLine(demos[demoToRun].Description);
                        // run the query
                        demos[demoToRun].Action.Invoke(null, new[] { application });
                    }
                    else
                    {
                        Console.WriteLine("Unknown Query Demo");
                    }
                }
            }
        }
        #endregion
    }
}