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
         RecordContinuouse();
         //StartTelemetricObserver();
         //Console.WriteLine("Starting");
         //using (DynamixelDriver driver = new DynamixelDriver("COM17"))
         //{
         //   byte[] servos = driver.Search(1, 10);
         //   foreach (var servo in servos)
         //   {
         //      driver.SetComplianceSlope(servo, ComplianceSlope.Default);
         //   }
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
            SetSpeedToAll(new byte[] { 1, 2, 3, 4 }, 100, driver);
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
                  DynamixelHelpers.MoveToAllBlocking(servoIds, positions, driver);
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

      public static void SetSpeedToAll(byte[] indexes, ushort speed, DynamixelDriver driver)
      {
         foreach (byte index in indexes)
         {
            driver.SetMovingSpeed(index, speed);
         }
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
