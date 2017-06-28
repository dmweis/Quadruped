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

      public static void WriteSingleMessage()
      {
         Console.WriteLine("Starting");
         using (SerialPort port = new SerialPort("COM17", 1000000))
         {
            port.Open();
            Thread.Sleep(400);
            byte id = 4;
            byte instruction = 3;
            byte parameter1 = 25;
            byte parameter2 = 0;

            byte[] data = EncodeMessage(id, instruction, new[] { parameter1, parameter2 });
            port.Write(data, 0, data.Length);
            Console.WriteLine("Sent");
            List<byte> buffer = new List<byte>();
            while (true)
            {
               buffer.Add((byte)port.ReadByte());
               bool flag = DecodeMessage(buffer.ToArray());
               if (flag)
               {
                  buffer.Clear();
               }
            }
         }
      }

      public static bool DecodeMessage(byte[] data)
      {
         if (data[0] != 0xFF || data[1] != 0xFF)
         {
            Console.WriteLine("not a packet");
            return false;
         }
         byte id = data[2];
         byte length = data[3];
         byte error = data[4];
         byte[] incoming = new byte[length-2];
         Array.Copy(data, 5,incoming, 0, length-2);
         byte checksum = data[data.Length - 1];
         int localChecksum = id + length + error + incoming.Sum(num => num);
         localChecksum = ~localChecksum;
         if (checksum!= localChecksum)
         {
            Console.WriteLine("Checksum error");
            return false;
         }
         Console.WriteLine("It worked!");
         return true;
      }

      public static byte[] EncodeMessage(byte id, byte instruction, byte[] parameters)
      {
         byte length = (byte) (2 + parameters.Length);
         int checksum = id + length + instruction + parameters.Sum(num => num);
         checksum = ~checksum;
         byte[] data = new byte[parameters.Length + 6];
         data[0] = 0xFF;
         data[1] = 0xFF;
         data[2] = id;
         data[3] = length;
         data[4] = instruction;
         for (int i = 0; i < parameters.Length; i++)
         {
            data[5 + i] = parameters[i];
         }
         data[data.Length - 1] = (byte) checksum;
         return data;
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
               List<ServoTelemetrics> servoData = new List<ServoTelemetrics>(); 
               foreach (var servo in servos)
               {
                  ServoTelemetrics telemetrics = new ServoTelemetrics();
                  telemetrics.Id = servo;
                  telemetrics.Voltage = (int)driver.GetVoltage(servo);
                  telemetrics.Temperature = driver.GetTemperature(servo);
                  telemetrics.Load = driver.GetPresentLoad(servo);
                  servoData.Add(telemetrics);
               }
               string json = JsonConvert.SerializeObject(servoData);
               channel.BasicPublish("DynamixelTelemetrics", string.Empty, null, Encoding.UTF8.GetBytes(json));
               Thread.Sleep(2 * 1000);
            }
         }
      }
   }
}
