using System;
using System.Threading;
using DynamixelServo.Driver;

namespace DynamixelServo.TestConsole
{
   class Program
   {
      static void Main(string[] args)
      {
         Console.WriteLine("Starting");
         using (DynamixelDriver driver = new DynamixelDriver("COM17"))
         {
            driver.SetTorque(1, false);
            driver.SetTorque(2, false);
            while (true)
            {
               driver.WriteGoalPosition(4,(ushort) (1024 - driver.ReadPresentPosition(1))); 
               driver.WriteGoalPosition(3,(ushort) (1024 - driver.ReadPresentPosition(2)));
               Thread.Sleep(50);
            }
         }
         Console.WriteLine("Press enter to exit");
         Console.ReadLine();
      }
   }
}
