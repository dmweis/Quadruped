using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using DynamixelServo.Driver;
using Newtonsoft.Json;
using RabbitMQ.Client;
using DynamixelServo.Quadruped;
using RabbitMQ.Client.Events;

namespace DynamixelServo.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            const string portName = "COM4";
            const ConsoleOptions defaultSelection = ConsoleOptions.GaitEngine;

            ConsoleOptions option = args.Length < 1 ? defaultSelection : (ConsoleOptions)Enum.Parse(typeof(ConsoleOptions), args[0]);
            while (option == ConsoleOptions.SelectOption)
            {
                Console.WriteLine("Select one of the following options:");
                foreach (var enumValue in Enum.GetValues(typeof(ConsoleOptions)))
                {
                    Console.WriteLine(enumValue);
                }
                Console.WriteLine();
                string input = string.Empty;
                try
                {
                    input = Console.ReadLine();
                    option = (ConsoleOptions)Enum.Parse(typeof(ConsoleOptions), input, true);
                }
                catch (Exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Invalid option: {input}");
                    Console.WriteLine("Try again");
                    Console.ResetColor();
                }
            }
            switch (option)
            {
                case ConsoleOptions.GaitEngine:
                    GaitEngineTest(portName);
                    break;
                case ConsoleOptions.OldIkEngine:
                    OldIkDriverTest(portName);
                    break;
                case ConsoleOptions.MeasureLimits:
                    MeasureLimits(portName);
                    break;
                case ConsoleOptions.TrackerListener:
                    TrackerListener(portName);
                    break;
                case ConsoleOptions.DriverTest:
                    DriverTest(portName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void DriverTest(string comPort)
        {
            Console.WriteLine("Starting");
            using (DynamixelDriver driver = new DynamixelDriver(comPort))
            {
                // test sync write feature
                byte[] ids = driver.Search(1, 20);
                ushort[] goals = new ushort[ids.Length];
                for (int index = 0; index < goals.Length; index++)
                {
                    goals[index] = 512;
                }
                driver.GroupSyncSetGoalPosition(ids, goals);
            }
            Console.WriteLine("Done");
        }

        private static void GaitEngineTest(string comPort)
        {
            Console.WriteLine("Starting");
            using (var driver = new DynamixelDriver(comPort))
            using (var quadruped = new QuadrupedIkDriver(driver))
            {
                LoadLimits(driver);
                using (var gaiteEngine = new BasicQuadrupedGaitEngine(quadruped))
                {
                    while (true)
                    {
                        if (GetCurrentConsoleKey().Key == ConsoleKey.Spacebar)
                        {
                            gaiteEngine.EnqueueOneStep(2, 2, 1, LegFlags.RfLrCross);
                        }
                        else
                        {
                            break;
                        }
                        if (GetCurrentConsoleKey().Key == ConsoleKey.Spacebar)
                        {
                            gaiteEngine.EnqueueOneStep(2, 2, 1, LegFlags.LfRrCross);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            Console.WriteLine("Done");
        }

        private static void LoadLimits(DynamixelDriver driver)
        {
            Console.WriteLine("Loading motor limits");
            List<MinMax> minMaxPairs = JsonConvert.DeserializeObject<List<MinMax>>(File.ReadAllText("MinMaxPairs.json"));
            foreach (var minMaxPair in minMaxPairs)
            {
                ushort maxAngle = DynamixelDriver.DegreesToUnits(minMaxPair.Max);
                ushort minAngle = DynamixelDriver.DegreesToUnits(minMaxPair.Min);
                driver.SetCcwMaxAngleLimit((byte)minMaxPair.Index, maxAngle);
                driver.SetCwMinAngleLimit((byte)minMaxPair.Index, minAngle);
            }
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(JsonConvert.SerializeObject(minMaxPairs));
            Console.ResetColor();
            Console.WriteLine("Limits loaded");
        }

        class MinMax
        {
            public float Min { get; set; } = float.MaxValue;
            public float Max { get; set; } = float.MinValue;
            public int Index { get; set; }

            public MinMax(int index)
            {
                Index = index;
            }
        }

        public static void OldIkDriverTest(string comPort)
        {
            Console.WriteLine("Starting");
            using (DynamixelDriver driver = new DynamixelDriver(comPort))
            using (QuadrupedIkDriver quadruped = new QuadrupedIkDriver(driver))
            {
                LoadLimits(driver);
                quadruped.Setup();
                quadruped.StandUpfromGround();
                Thread.Sleep(1000);
                Console.Beep();
                bool keepGoing = true;
                while (keepGoing)
                {
                    ConsoleKeyInfo keyInfo = GetCurrentConsoleKey();
                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.LeftArrow:
                        case ConsoleKey.A:
                            if (keyInfo.Modifiers == ConsoleModifiers.Shift)
                            {
                                quadruped.TurnLeft();
                            }
                            else
                            {
                                quadruped.TurnLeftSlow();
                            }
                            break;
                        case ConsoleKey.RightArrow:
                        case ConsoleKey.D:
                            if (keyInfo.Modifiers == ConsoleModifiers.Shift)
                            {
                                quadruped.TurnRight();
                            }
                            else
                            {
                                quadruped.TurnRightSlow();
                            }
                            break;
                        case ConsoleKey.UpArrow:
                        case ConsoleKey.W:
                            // call forward movement
                            break;
                        case ConsoleKey.DownArrow:
                        case ConsoleKey.S:
                            quadruped.RelaxedStance();
                            break;
                        case ConsoleKey.Spacebar:
                            Console.WriteLine("Enter x y z");
                            float[] input = Console
                                .ReadLine()
                                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(float.Parse)
                                .ToArray();
                            float x = input[0];
                            float y = input[1];
                            float z = input[2];
                            quadruped.MoveRelativeCenterMass(new Vector3(x, y, z));
                            break;
                        case ConsoleKey.Escape:
                            keepGoing = false;
                            break;
                    }
                }
                Console.Beep();
                quadruped.DisableMotors();
            }
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        private static void MeasureLimits(string comPort)
        {
            Console.WriteLine("Starting");
            using (DynamixelDriver driver = new DynamixelDriver(comPort))
            {
                var servos = driver.Search(0, 20);
                // list of min maxes
                List<MinMax> minMaxPairs = new List<MinMax>();
                for (int index = 0; index < servos.Length; index++)
                {
                    minMaxPairs.Add(new MinMax(index + 1));
                }

                // read it from file
                minMaxPairs = JsonConvert.DeserializeObject<List<MinMax>>(File.ReadAllText("MinMaxPairs.json"));
                foreach (var servo in servos)
                {
                    driver.SetTorque(servo, false);
                }
                while (true)
                {
                    string serovStatuses = String.Empty; ;
                    foreach (var servo in servos)
                    {
                        float posInDegrees = driver.GetPresentPositionInDegrees(servo);
                        if (servo == 1)
                        {
                            serovStatuses += $" {servo}-{posInDegrees}";
                        }
                        if (posInDegrees < minMaxPairs[servo - 1].Min)
                        {
                            minMaxPairs[servo - 1].Min = posInDegrees;
                        }
                        if (posInDegrees > minMaxPairs[servo - 1].Max)
                        {
                            minMaxPairs[servo - 1].Max = posInDegrees;
                        }
                    }
                    Console.WriteLine(serovStatuses);
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                        while (Console.KeyAvailable)
                        {
                            keyInfo = Console.ReadKey(true);
                        }
                        if (keyInfo.Key == ConsoleKey.Escape)
                        {
                            break;
                        }
                    }
                }
                // after loop
                string json = JsonConvert.SerializeObject(minMaxPairs);
                Console.WriteLine(json);
                File.WriteAllText("MinMaxPairs.json", json);
            }
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
            Console.WriteLine("Done");
        }

        public static void TrackerListener(string comPort)
        {
            Console.WriteLine("Starting");
            ConnectionFactory factory = new ConnectionFactory() { HostName = "localhost" };
            using (IConnection connection = factory.CreateConnection())
            using (IModel channel = connection.CreateModel())
            using (DynamixelDriver driver = new DynamixelDriver(comPort))
            using (QuadrupedIkDriver quadruped = new QuadrupedIkDriver(driver))
            {
                quadruped.Setup();
                quadruped.StandUpfromGround();
                string queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queueName, "TrackerService", string.Empty);
                EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
                channel.BasicConsume(consumer, queueName);
                consumer.Received += (sender, args) =>
                {
                    DeviceTrackingData trackingData =
                        JsonConvert.DeserializeObject<DeviceTrackingData>(Encoding.UTF8.GetString(args.Body));
                    Console.WriteLine($"Position {trackingData.Position}");
                    Vector3 correctedPos = new Vector3(trackingData.Position.X, -trackingData.Position.Z, trackingData.Position.Y);
                    // validate
                    if (Math.Abs(correctedPos.X) > 8 || Math.Abs(correctedPos.Y) > 8 || correctedPos.Z > 4.5 || correctedPos.Z < -7.5)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error! {correctedPos}");
                        Console.ResetColor();
                        return;
                    }
                    quadruped.MoveAbsoluteCenterMass(correctedPos, 15, -13);
                };
                Console.WriteLine("Enter to stop");
                Console.ReadLine();
            }
            Console.WriteLine("Done");
        }

        class DeviceTrackingData
        {
            public int DeviceIndex { get; set; }
            public Vector3 Position { get; set; }
            public Quaternion Rotation { get; set; }
        }

        private static ConsoleKeyInfo GetCurrentConsoleKey()
        {
            while (Console.KeyAvailable)
            {
                Console.ReadKey(true);
            }
            return Console.ReadKey(true);
        }
    }
}
