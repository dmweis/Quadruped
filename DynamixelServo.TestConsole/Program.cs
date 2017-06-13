using System;
using System.Collections.Generic;
using System.Threading;
using DynamixelServo.Driver;
using Timer = System.Timers.Timer;

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
         //   driver.SetMovingSpeed(1, 0, DynamixelProtocol.Version2);
         //   driver.SetGoalPosition(1, 0, DynamixelProtocol.Version2);
         //   Thread.Sleep(1000);
         //   driver.SetGoalPosition(1, 1023, DynamixelProtocol.Version2);
         //   Thread.Sleep(1000);
         //   driver.SetGoalPosition(1, 0, DynamixelProtocol.Version2);
         //}
         //Console.WriteLine("Press enter to exit");
         //Console.ReadLine();
      }

      public static void RecordContinuouse()
      {
         Console.WriteLine("Starting");
         using (DynamixelDriver driver = new DynamixelDriver("COM17"))
         {
            byte[] servoIds = driver.Search(1, 10);
            IList<ushort[]> history = new List<ushort[]>();
            foreach (var id in servoIds)
            {
               driver.SetMovingSpeed(id, 100);
               driver.SetTorque(id, false);
            }
            Console.WriteLine("press enter to start recording");
            Console.ReadLine();
            using (Timer timer = new Timer())
            {
               timer.Interval = 100;
               timer.AutoReset = true;
               timer.Elapsed += (sender, args) =>
               {
                  ushort[] currentPositions = new ushort[servoIds.Length];
                  for (int i = 0; i < servoIds.Length; i++)
                  {
                     currentPositions[i] = driver.ReadPresentPosition(servoIds[i]);
                  }
                  history.Add(currentPositions);
               };
               Console.WriteLine("Started recording. Press enter to stop!");
               timer.Start();
               Console.ReadLine();
               timer.AutoReset = false;
               Thread.Sleep(200);
            }
            while (true)
            {
               foreach (var positions in history)
               {
                  DynamixelHelpers.MoveToAll(servoIds, positions, driver);
                  Thread.Sleep(100);
               }
               Console.WriteLine("write q to exit");
               string input = Console.ReadLine();
               if (input == "q")
               {
                  break;
               }
            }
         }
         Console.WriteLine("Press enter to exit");
         Console.ReadLine();
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
