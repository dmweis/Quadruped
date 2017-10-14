using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using BrandonPotter.XBox;
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
            const ConsoleOptions defaultSelection = ConsoleOptions.BasicGaitEngineXbox;

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
                case ConsoleOptions.MeasureLimits:
                    MeasureLimits(portName);
                    break;
                case ConsoleOptions.TrackerListener:
                    TrackerListener(portName);
                    break;
                case ConsoleOptions.DriverTest:
                    DriverTest(portName);
                    break;
                case ConsoleOptions.BasicGaitEngine:
                    BasicGaitEngineTest(portName);
                    break;
                case ConsoleOptions.BasicGaitEngineXbox:
                    BasicGaitEngineXboxTest(portName);
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
                using (var gaiteEngine = new FunctionalQuadrupedGaitEngine(quadruped))
                {
                    while (GetCurrentConsoleKey().Key != ConsoleKey.Escape)
                    {
                        
                    }
                }
            }
            Console.WriteLine("Done");
        }

        private static void BasicGaitEngineTest(string comPort)
        {
            Console.WriteLine("Starting");
            using (var driver = new DynamixelDriver(comPort))
            using (var quadruped = new QuadrupedIkDriver(driver))
            {
                LoadLimits(driver);
                using (var gaiteEngine = new BasicQuadrupedGaitEngine(quadruped))
                {
                    bool keepGoing = true;
                    LegFlags nextLegCombo = LegFlags.RfLrCross;
                    while (keepGoing)
                    {
                        nextLegCombo = nextLegCombo == LegFlags.RfLrCross ? LegFlags.LfRrCross : LegFlags.RfLrCross;
                        Vector2 direction = Vector2.Zero;
                        ConsoleKeyInfo keyInfo = GetCurrentConsoleKey();
                        switch (keyInfo.Key)
                        {
                            case ConsoleKey.LeftArrow:
                            case ConsoleKey.A:
                                direction = new Vector2(-1, 0);
                                break;
                            case ConsoleKey.RightArrow:
                            case ConsoleKey.D:
                                direction = new Vector2(1, 0);
                                break;
                            case ConsoleKey.UpArrow:
                            case ConsoleKey.W:
                                direction = new Vector2(0, 1);
                                break;
                            case ConsoleKey.DownArrow:
                            case ConsoleKey.S:
                                direction = new Vector2(0, -1);
                                break;
                            case ConsoleKey.D3:
                                gaiteEngine.EnqueueOneRotation(keyInfo.Modifiers == ConsoleModifiers.Shift ? 25 : 12.5f, nextLegCombo);
                                break;
                            case ConsoleKey.D2:
                                gaiteEngine.EnqueueOneRotation(keyInfo.Modifiers == ConsoleModifiers.Shift ? -25 : -12.5f, nextLegCombo);
                                break;
                            case ConsoleKey.Q:
                                direction = keyInfo.Modifiers == ConsoleModifiers.Shift ? new Vector2(-1, -1) : new Vector2(-1, 1);
                                break;
                            case ConsoleKey.E:
                                direction = keyInfo.Modifiers == ConsoleModifiers.Shift ? new Vector2(1, -1) : new Vector2(1, 1);
                                break;
                            case ConsoleKey.Z:
                                direction = new Vector2(-1, -1);
                                break;
                            case ConsoleKey.C:
                                direction = new Vector2(1, -1);
                                break;
                            case ConsoleKey.Spacebar:
                                gaiteEngine.EnqueueOneRotation(0);
                                break;
                            case ConsoleKey.Escape:
                                keepGoing = false;
                                break;
                        }
                        gaiteEngine.EnqueueOneStep(direction, nextLegCombo);
                        gaiteEngine.WaitUntilCommandQueueIsEmpty();
                    }
                }
            }
            Console.WriteLine("Done");
        }

        private static void BasicGaitEngineXboxTest(string comPort)
        {
            Console.WriteLine("Starting");
            using (var driver = new DynamixelDriver(comPort))
            using (var quadruped = new QuadrupedIkDriver(driver))
            {
                LoadLimits(driver);
                using (var gaiteEngine = new BasicQuadrupedGaitEngine(quadruped))
                {
                    var connectedController = XBoxController.GetConnectedControllers().FirstOrDefault();
                    if (connectedController == null)
                    {
                        throw new InvalidOperationException("Xbox controller not detected!");
                    }
                    LegFlags nextLegCombo = LegFlags.RfLrCross;
                    while (true)
                    {
                        Vector2 direction = Vector2.Zero;
                        direction.X = (float)connectedController.ThumbLeftX - 50;
                        direction.Y = (float)connectedController.ThumbLeftY - 50;
                        if (connectedController.ButtonBackPressed)
                        {
                            break;
                        }
                        double rightStick = connectedController.ThumbRightX - 50;
                        if (rightStick != 0)
                        {
                            gaiteEngine.EnqueueOneRotation((float)(rightStick / 2), nextLegCombo);
                            nextLegCombo = nextLegCombo == LegFlags.RfLrCross ? LegFlags.LfRrCross : LegFlags.RfLrCross;
                            gaiteEngine.WaitUntilCommandQueueIsEmpty();
                        }
                        else if (direction != Vector2.Zero)
                        {
                            gaiteEngine.EnqueueOneStep(direction, nextLegCombo);
                            nextLegCombo = nextLegCombo == LegFlags.RfLrCross ? LegFlags.LfRrCross : LegFlags.RfLrCross;
                            gaiteEngine.WaitUntilCommandQueueIsEmpty();
                        }
                        else
                        {
                           Thread.Sleep(20);
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
                    string serovStatuses = string.Empty;
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
