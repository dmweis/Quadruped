using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using DynamixelServo.Driver;
using Newtonsoft.Json;
using RabbitMQ.Client;
using DynamixelServo.Quadruped;
using RabbitMQ.Client.Events;
using Timer = System.Timers.Timer;

namespace DynamixelServo.TestConsole
{
    class Program
    {
        private static ConsoleKeyInfo _lastPressedKeyInfo;
        static void Main(string[] args)
        {
            TrackerListener();
            Environment.Exit(0);
            Console.WriteLine("Starting");
            //StartTelemetricObserver();
            using (DynamixelDriver driver = new DynamixelDriver("COM4"))
            using (QuadrupedIkDriver quadruped = new QuadrupedIkDriver(driver))
            {
                quadruped.Setup();
                quadruped.StandUpfromGround();
                Thread.Sleep(1000);
                Console.Beep();
                bool keepGoing = true;
                while (keepGoing)
                {
                    switch (GetCurrentConsoleKey())
                    {
                        case ConsoleKey.LeftArrow:
                        case ConsoleKey.A:
                            if (_lastPressedKeyInfo.Modifiers == ConsoleModifiers.Shift)
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
                            if (_lastPressedKeyInfo.Modifiers == ConsoleModifiers.Shift)
                            {
                                quadruped.TurnRight();
                            }
                            else
                            {
                                quadruped.TurnRightSlow();
                            }
                            break;
                        case ConsoleKey.UpArrow:
                            quadruped.ForwardSimple();
                            break;
                        case ConsoleKey.W:
                            quadruped.ForwardCreeper();
                            break;
                        case ConsoleKey.E:
                            quadruped.ForwardCreeperStable();
                            break;
                        case ConsoleKey.DownArrow:
                        case ConsoleKey.S:
                            quadruped.RelaxedStance();
                            break;
                        case ConsoleKey.Spacebar:
                            Console.WriteLine("Enter x y z");
                            float[] input = Console
                                .ReadLine()
                                .Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries)
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
            //Console.ReadLine();
        }

        private static void GaitEngineTest()
        {
            Console.WriteLine("Starting");
            using (DynamixelDriver driver = new DynamixelDriver("COM4"))
            using (QuadrupedIkDriver quadruped = new QuadrupedIkDriver(driver))
            using (BasicQuadrupedGaitEngine gaiteEngine = new BasicQuadrupedGaitEngine(quadruped))
            {                
                Console.WriteLine("Enter to stop");
                Console.ReadLine();
            }
            Console.WriteLine("Done");
        }

        public static void TrackerListener()
        {
            Console.WriteLine("Starting");
            ConnectionFactory factory = new ConnectionFactory() { HostName = "localhost" };
            using (IConnection connection = factory.CreateConnection())
            using (IModel channel = connection.CreateModel())
            using (DynamixelDriver driver = new DynamixelDriver("COM4"))
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
                }
            }
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        public static void StartTelemetricObserver()
        {
            Console.WriteLine("Starting");
            ConnectionFactory factory = new ConnectionFactory() { HostName = "localhost" };
            using (IConnection connection = factory.CreateConnection())
            using (IModel channel = connection.CreateModel())
            using (DynamixelDriver driver = new DynamixelDriver("COM4"))
            {
                channel.ExchangeDeclare("DynamixelTelemetrics", "fanout");
                byte[] servos = driver.Search(1, 20);
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

        private static void ReportServoPosition()
        {
            Console.WriteLine("Starting");
            using (IConnection connection = new ConnectionFactory { HostName = "localhost" }.CreateConnection())
            using (IModel channel = connection.CreateModel())
            using (DynamixelDriver driver = new DynamixelDriver("COM4"))
            {
                channel.ExchangeDeclare("ArmPositionUpdate", "fanout");
                foreach (var servo in driver.Search(1, 5))
                {
                    driver.SetTorque(servo, false);
                }
                while (true)
                {
                    float @base = DynamixelDriver.UnitsToDegrees(driver.GetPresentPosition(1));
                    float shoulder = DynamixelDriver.UnitsToDegrees(driver.GetPresentPosition(2));
                    float elbow = DynamixelDriver.UnitsToDegrees(driver.GetPresentPosition(3));
                    float wrist = DynamixelDriver.UnitsToDegrees(driver.GetPresentPosition(4));
                    byte[] body =
                        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new ArmPosition(@base, shoulder, elbow, wrist)));
                    channel.BasicPublish("ArmPositionUpdate", "", body: body);
                    Thread.Sleep(100);
                }

            }
        }

        private static ConsoleKey GetCurrentConsoleKey()
        {
            while (Console.KeyAvailable)
            {
                Console.ReadKey(true);
            }
            _lastPressedKeyInfo = Console.ReadKey(true);
            return _lastPressedKeyInfo.Key;
        }
    }

    class ArmPosition
    {
        public float Base { get; set; }
        public float Shoulder { get; set; }
        public float Elbow { get; set; }
        public float Wrist { get; set; }

        public ArmPosition(float @base, float shoulder, float elbow, float wrist)
        {
            Base = @base;
            Shoulder = shoulder;
            Elbow = elbow;
            Wrist = wrist;
        }
    }

    class DeviceTrackingData
    {
        public int DeviceIndex { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
    }
}
