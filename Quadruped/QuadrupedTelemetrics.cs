using System.Collections.Generic;
using System.Linq;
using Quadruped.Driver;

namespace Quadruped
{
    public class QuadrupedTelemetrics
    {
        public IEnumerable<ServoTelemetrics> ServoTelemetrics { get; set; }

        public float AverageTemperature => (float)ServoTelemetrics.Average(servo => servo.Temperature);

        public float AverageVoltage => ServoTelemetrics.Average(servo => servo.Voltage);
    }
}
