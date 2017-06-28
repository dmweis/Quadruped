using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using DynamixelServo.Driver;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Timer = System.Timers.Timer;

namespace DynamixelServo.TestConsole
{
   class Program
   {
      static void Main(string[] args)
      {
         Console.WriteLine("Starting");
         using (DynamixelDriver driver = new DynamixelDriver("COM17"))
         {
            driver.SetWheelMode(1);
            driver.SetMovingSpeed(1, 1023, true);
            Console.WriteLine("Moving CW");
            Console.ReadLine();
            driver.SetMovingSpeed(1, 1023, false);
            Console.WriteLine("Moving CCW");
            Console.ReadLine();
            driver.SetMovingSpeed(1, 0, true);
            driver.SetServoMode(1);
            Console.WriteLine("Move?");
            Console.ReadLine();
            driver.MoveToBlocking(1, 0);
            driver.MoveToBlocking(1, 1023);
            Console.WriteLine("Bye");

         }
         Console.WriteLine("Press enter to exit");
         Console.ReadLine();
      }

      public static void RecordContinuouse()
      {
         Console.WriteLine("Starting");
         using (DynamixelDriver driver = new DynamixelDriver("COM17"))
         {
            byte[] servoIds = driver.Search(1, 10);
            IList<ushort[]> history = new List<ushort[]>();
            driver.SetMovingSpeed(1, 100);
            driver.SetMovingSpeed(2, 250);
            driver.SetMovingSpeed(3, 250);
            driver.SetMovingSpeed(4, 250);
            foreach (var id in servoIds)
            {
               driver.SetComplianceSlope(id, ComplianceSlope.S128);
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
                     currentPositions[i] = driver.GetPresentPosition(servoIds[i]);
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
                  driver.MoveToAll(servoIds, positions);
                  Thread.Sleep(100);
               }
               Console.WriteLine("write q to exit");
               string input = Console.ReadLine();
               if (input == "q")
               {
                  break;
               }
            }
            foreach (var servoId in servoIds)
            {
               driver.SetTorque(servoId, false);
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
            driver.SetMovingSpeed(1, 100);
            driver.SetMovingSpeed(2, 200);
            driver.SetMovingSpeed(3, 200);
            driver.SetMovingSpeed(4, 200);

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
                  currentPositions[i] = driver.GetPresentPosition(servoIds[i]);
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
                  driver.MoveToAllBlocking(servoIds, positions);
               }
               Console.WriteLine("write esc to exit");
               string input = Console.ReadLine();
               if (input == "esc")
               {
                  break;
               }
               //ushort speed = ushort.Parse(input);
               //SetSpeedToAll(new byte[] {2,3,4}, speed, driver);
            }
         }
         Console.WriteLine("Press enter to exit");
         Console.ReadLine();
      }

      public static void StartTelemetricObserver()
      {
         Console.WriteLine("Starting");
         ConnectionFactory factory = new ConnectionFactory(){HostName = "localhost"};
         IConnection connection = factory.CreateConnection();
         IModel channel = connection.CreateModel();
         channel.ExchangeDeclare("DynamixelTelemetrics", "fanout");
         using (DynamixelDriver driver = new DynamixelDriver("COM17"))
         {
            byte[] servos = driver.Search(1, 10);
            while (true)
            {
               Console.WriteLine("Publising");
               IEnumerable<ServoTelemetrics> servoData = servos.Select(servoIndex => driver.GetTelemetrics(servoIndex));
               string json = JsonConvert.SerializeObject(servoData);
               channel.BasicPublish("DynamixelTelemetrics", string.Empty, null, Encoding.UTF8.GetBytes(json));
               Thread.Sleep(2 * 1000);
            }
         }
      }
   }
}
