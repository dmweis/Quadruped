using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamixelServo.Driver
{
   class ServoTelemetrics
   {
      public int Id { get; set; }
      public int Temperature { get; set; }
      public float Voltage { get; set; }
      public int Load { get; set; }

   }
}
