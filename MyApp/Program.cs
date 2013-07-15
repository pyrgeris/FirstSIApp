
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

        private static int AskUserWhichQuery()
        {
            Console.WriteLine();
            Console.WriteLine("Pick a query:");
            Console.WriteLine("0. Pass-through");
            Console.WriteLine("1. Hop-Count");
            Console.WriteLine("2. Exit");

            string queryNumStr = Console.ReadLine();
            int queryNum;

            try
            {
                queryNum = Int32.Parse(queryNumStr, new CultureInfo("en-US"));
            }
            catch (FormatException)
            {
                queryNum = 2;
            }

            return queryNum;
        }

        static void PassThrough()
        {
            var inputStream = CepStream<MyReading>.Create("MyStream");
            // Define input stream object, mapped to stream names
            // Instantiate an adapter from the input factory class
            var queryStream = from e in inputStream select e;
            BindAndRunQuery(
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
            BindAndRunQuery(
                "HopCount",
                queryStream,
                EventShape.Interval,
                new List<string>() { "input1", "input2", "input3", "input4", "input5" },
                new List<string>() { "Count" });

        }


        private static void BindAndRunQuery<TPayload>(string queryName, CepStream<TPayload> queryStream, EventShape outputEventShape, List<string> inputFields, List<string> outputFields)
        {
            var inputConfig = new CsvInputConfig
            {
                InputFileName = @"..\MyApp\Input Data\input.csv",
                Delimiter = new char[] { ',' },
                BufferSize = 4096,
                CtiFrequency = 1,
                CultureName = "en-US",
                Fields = inputFields,
                NonPayloadFieldCount = 0,
                StartTimePos = 1,
                EndTimePos = 6
            };

            // The adapter recognizes empty filename as a write to console.
            var outputConfig = new CsvOutputConfig
            {
                OutputFileName = @"..\MyApp\Output Data\output.txt",
                Delimiter = new string[] { "\t" },
                CultureName = "en-US",
                Fields = outputFields
            };

            // Note - Please change the instance name to the one you have chosen during installation
            using (var server = Server.Create("Instance1"))
            {
                Application application = server.CreateApplication("MyApp");

                // set up input and output adapters
                InputAdapter inputAdapter = application.CreateInputAdapter<CsvInputFactory>("input", "Csv Input Source");
                OutputAdapter outputAdapter = application.CreateOutputAdapter<CsvOutputFactory>("output", "Csv Output");

                // set up the query template
                QueryTemplate queryTemplate = application.CreateQueryTemplate("QueryTemplate", string.Empty, queryStream);

                // set up advance time settings to enqueue CTIs
                var advanceTimeGenerationSettings = new AdvanceTimeGenerationSettings(inputConfig.CtiFrequency, TimeSpan.FromMilliseconds(200), true);
                var advanceTimeSettings = new AdvanceTimeSettings(advanceTimeGenerationSettings, null, AdvanceTimePolicy.Adjust);

                // Bind query template to input and output streams
                QueryBinder queryBinder = new QueryBinder(queryTemplate);
                queryBinder.BindProducer<MyReading>("MyStream", inputAdapter, inputConfig, EventShape.Point, advanceTimeSettings);
                queryBinder.AddConsumer("outputStream", outputAdapter, outputConfig, outputEventShape, StreamEventOrder.FullyOrdered);

                // Create a runnable query by binding the query template to the input stream of interval events,
                // and to an output stream of fully ordered point events (through an output adapter instantiated
                // from the output factory class)
                Query query = application.CreateQuery(queryName, "Query", queryBinder);

                RunQuery(query);
            }
        }

        private static void RunQuery(Query query)
        {
            query.Start();

            Console.WriteLine("***Hit Return to exit after viewing query output***");
            Console.WriteLine();
            Console.ReadLine();

            query.Stop();
        }


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