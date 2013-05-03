using Microsoft.ComplexEventProcessing.Linq;
using Microsoft.ComplexEventProcessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
namespace MyApp
{
    public class newSensorReading
    {
        public double input1 { get; set; }
        public double input2 { get; set; }
        public double input3 { get; set; }
        public double input4 { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public override string ToString()
        {
            return new { input1, input2, input3, input4 }.ToString();
        }


        public static IEnumerable<newSensorReading> getSensorReadings(string filename)
        {
            CultureInfo _culture = new CultureInfo("en-US");

            List<newSensorReading> events = new List<newSensorReading>();

            string[] allLines = File.ReadAllLines(filename);

            foreach (var s in allLines)
            {
                if (s.Equals(""))
                {
                    Console.WriteLine("Blank string man");
                }
                String[] data = s.Split(',');

                //Console.WriteLine(s);

                newSensorReading e = new newSensorReading();

                e.StartTime = DateTime.Now.ToUniversalTime();
                e.EndTime = DateTime.Now.AddTicks(1000).ToUniversalTime();
                e.input1 = Convert.ToDouble(data[0]);
                e.input2 = Convert.ToDouble(data[1]);
                e.input3 = Convert.ToDouble(data[2]);
                e.input4 = Convert.ToDouble(data[3]);

                events.Add(e);
            }
            return events;
        }
    }   
}
