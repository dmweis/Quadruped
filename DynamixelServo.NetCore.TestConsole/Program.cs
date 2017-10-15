using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using DynamixelServo.Driver;
using DynamixelServo.Quadruped;
using Newtonsoft.Json;

namespace DynamixelServo.NetCore.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            Console.WriteLine("Starting");
            using (var driver = new DynamixelDriver("COM4"))
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
