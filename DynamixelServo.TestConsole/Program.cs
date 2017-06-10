using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            byte[] servos = driver.Search(1, 10);
            Console.WriteLine("Leds on");
            Console.WriteLine("Press enter to turn leds on");
            Console.ReadLine();
            foreach (byte servo in servos)
            {
               driver.SetLed(servo, true);
            }
            Console.WriteLine("Press enter to turn leds off");
            Console.ReadLine();
            foreach (byte servo in servos)
            {
               driver.SetLed(servo, false);
            }
            Console.WriteLine("Leds off");
         }
         Console.WriteLine("Press enter to exit");
         Console.ReadLine();
      }
   }
}
