using System;
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
            using (var controller = new SmartQuadrupedController(gaiteEngine))
            {
                controller.
                Console.WriteLine("Done!");
                Console.ReadLine();
            }
            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }
    }
}
