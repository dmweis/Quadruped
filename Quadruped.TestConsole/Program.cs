using System;
using System.Numerics;
using System.Threading;
using Quadruped.Driver;

namespace Quadruped.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting");
            using (var driver = new DynamixelDriver("COM4"))
            using (var ikDriver = new QuadrupedIkDriver(driver))
            using (var controller = new SmartQuadrupedController(ikDriver))
            {
                //controller.Speed = 30;
                var moveDistance = 200;
                //controller.Direction = Vector2.UnitX;
                controller.Rotation = 10;
                Thread.Sleep(2000);
                controller.Direction = Vector2.Zero;
                controller.Rotation = 0;

                Console.WriteLine(controller.Odometry);
                Console.WriteLine("Done!");
                Console.ReadLine();
                controller.Stop();
            }
            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }
    }
}
