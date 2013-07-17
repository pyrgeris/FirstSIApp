
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


    public class BindandQuery
    {
        public static void BindAndRunQuery<TPayload>(string queryName, CepStream<TPayload> queryStream, EventShape outputEventShape, List<string> inputFields, List<string> outputFields)
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
                OutputFileName = @"..\MyApp\Output Data\output.csv",
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

        public static void RunQuery(Query query)
        {
            query.Start();

            Console.WriteLine("***Hit Return to exit after viewing query output***");
            Console.WriteLine();
            Console.ReadLine();

            query.Stop();
        }


    }
}