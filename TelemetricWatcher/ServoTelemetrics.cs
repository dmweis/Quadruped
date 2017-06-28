using System.Collections.Generic;
using Newtonsoft.Json;

namespace TelemetricWatcher
{
   class ServoTelemetrics
   {
      public int Id { get; set; }
      public int Temperature { get; set; }
      public float Voltage { get; set; }
      public int Load { get; set; }

      public static ServoTelemetrics Deserealize(string json)
      {
         return JsonConvert.DeserializeObject<ServoTelemetrics>(json);
      }

      public static ICollection<ServoTelemetrics> DeserealizeCollection(string json)
      {
         return JsonConvert.DeserializeObject<ICollection<ServoTelemetrics>>(json);
      }
   }
}
