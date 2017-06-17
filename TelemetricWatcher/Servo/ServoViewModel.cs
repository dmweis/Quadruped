using System;
using GalaSoft.MvvmLight;

namespace TelemetricWatcher.Servo
{
   class ServoViewModel : ViewModelBase
   {
      private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

      private int _id;
      public int Id
      {
         get => _id;
         set => Set(ref _id, value);
      }

      private float _temperature;
      public float Temperature
      {
         get => _temperature;
         set => Set(ref _temperature, value); 
      }

      private float _voltage;
      public float Voltage
      {
         get => _voltage;
         set => Set(ref _voltage, value);
      }

      private float _load;
      public float Load
      {
         get => _load;
         set => Set(ref _load, value);
      }

      private DateTime _lastUpDateTime = DateTime.Now;

      public ServoViewModel(int id)
      {
         Id = id;
      }

      public void Update(ServoTelemetrics telemetrics)
      {
         if (telemetrics?.Id == Id)
         {
            Temperature = telemetrics.Temperature;
            Voltage = (float)telemetrics.Voltage / 10;
            // convert load
            bool ccw = Convert.ToBoolean(telemetrics.Load & 1 << 10);
            float load = (float)(telemetrics.Load & ~(1 << 10)) / 10;
            Load = ccw ? -load : load;
            _lastUpDateTime = DateTime.Now;
         }
      }

      public bool IsOld()
      {
         return DateTime.Now - _lastUpDateTime > Timeout;
      }
   }
}
