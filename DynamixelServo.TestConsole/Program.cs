using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using DynamixelServo.Driver;

namespace DynamixelServo.TestConsole
{
   class Program
   {
      static void Main(string[] args)
      {
         Record();
         //Console.WriteLine("Starting");
         //using (DynamixelDriver driver = new DynamixelDriver("COM17"))
         //{
         //   byte[] servos = driver.Search(1, 10);
         //   foreach (var servo in DynamixelHelpers.IterateLeds(servos, driver))
         //   {
         //      Console.WriteLine(servo);
         //      Console.ReadLine();
         //   }
         //}
         //Console.WriteLine("Press enter to exit");
         //Console.ReadLine();
      }

      public static void Record()
      {
         Console.WriteLine("Starting");
         using (DynamixelDriver driver = new DynamixelDriver("COM17"))
         {
            byte[] servoIds = driver.Search(1, 10);
            IList<ushort[]> history = new List<ushort[]>();
            foreach (var id in servoIds)
            {
               driver.SetTorque(id, false);
            }
            Console.WriteLine("press enter to start recording");
            Console.ReadLine();
            while (true)
            {
               ushort[] currentPositions = new ushort[servoIds.Length];
               for (int i = 0; i < servoIds.Length; i++)
               {
                  currentPositions[i] = driver.ReadPresentPosition(servoIds[i]);
               }
               history.Add(currentPositions);
               Console.WriteLine("step");
               var input = Console.ReadLine();
               if (input == "done")
               {
                  break;
               }
            }
            while (true)
            {
               foreach (var positions in history)
               {
                  DynamixelHelpers.MoveToAllBlocking(servoIds, positions, driver);
               }
               Console.WriteLine("write esc to exit");
               string input = Console.ReadLine();
               if (input == "esc")
               {
                  break;
               }
            }
         }
         Console.WriteLine("Press enter to exit");
         Console.ReadLine();
      }
   }
}
