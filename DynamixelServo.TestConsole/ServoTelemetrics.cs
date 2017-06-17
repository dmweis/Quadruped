using System.Collections.Generic;
using Newtonsoft.Json;

namespace DynamixelServo.TestConsole
{
   class ServoTelemetrics
   {
      public int Id { get; set; }
      public int Temperature { get; set; }
      public int Voltage { get; set; }
      public int Load { get; set; }

      public string Serealize()
      {
         return JsonConvert.SerializeObject(this);
      }

      public static string SerealizeCollection(ICollection<ServoTelemetrics> collection)
      {
         return JsonConvert.SerializeObject(collection);
      }
   }
}
