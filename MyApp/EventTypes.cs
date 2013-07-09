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

            int count = 0;

            foreach (var s in allLines)
            {
                if (s.Equals(""))
                {
                    Console.WriteLine("Blank string");
                }
                String[] data = s.Split(',');

                newSensorReading e = new newSensorReading();

                e.StartTime = DateTime.ParseExact("19891220 14:00:00", "yyyyMMdd HH:mm:ss", DateTimeFormatInfo.InvariantInfo).AddMilliseconds(200 * count).ToUniversalTime();
                e.EndTime = e.StartTime;
                e.input1 = double.Parse(data[0], System.Globalization.CultureInfo.InvariantCulture);
                e.input2 = double.Parse(data[1], System.Globalization.CultureInfo.InvariantCulture);
                e.input3 = double.Parse(data[2], System.Globalization.CultureInfo.InvariantCulture);
                e.input4 = double.Parse(data[3], System.Globalization.CultureInfo.InvariantCulture);

                count++;

                events.Add(e);
            }
            return events;
        }
    }   
}
