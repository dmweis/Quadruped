using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DynamixelServo.Driver
{
   public class DynamixelHelpers
   {
      public static IEnumerable<byte> IterateLeds(byte[] indexes, DynamixelDriver driver)
      {
         foreach (byte index in indexes)
         {
            driver.SetLed(index, true);
            yield return index;
            driver.SetLed(index, false);
         }
      }

      public static byte SwitchId(byte servo1Id, byte servo2Id, DynamixelDriver driver)
      {
         // find empty index
         byte freeIndex = 255;
         for (byte index = 1; index < 252; index++)
         {
            if (!driver.Ping(index))
            {
               freeIndex = index;
               break;
            }
         }
         if (freeIndex == 255)
         {
            throw new InvalidOperationException("Can't find free ID");
         }
         driver.SetId(servo1Id, freeIndex);
         driver.SetId(servo2Id, servo1Id);
         driver.SetId(freeIndex, servo2Id);
         return freeIndex;
      }

      public static void MoveToBlocking(byte servoId, ushort goal,DynamixelDriver driver)
      {
         driver.SetGoalPosition(servoId, goal);
         while (driver.IsMoving(servoId))
         {
            Thread.Sleep(50);
         }
      }

      public static void MoveToAll(byte[] servoIds, ushort[] goals, DynamixelDriver driver)
      {
         if (servoIds.Length != goals.Length)
         {
            throw new ArgumentException($"{nameof(servoIds)} and {nameof(goals)} have to be the same length");
         }
         for (int index = 0; index < servoIds.Length; index++)
         {
            driver.SetGoalPosition(servoIds[index], goals[index]);
         }
      }

      public static void MoveToAllBlocking(byte[] servoIds, ushort[] goals, DynamixelDriver driver)
      {
         if (servoIds.Length != goals.Length)
         {
            throw new ArgumentException($"{nameof(servoIds)} and {nameof(goals)} have to be the same length");
         }
         for (int index = 0; index < servoIds.Length; index++)
         {
            driver.SetGoalPosition(servoIds[index], goals[index]);
         }
         while (!servoIds.All(index => !driver.IsMoving(index)))
         {
            Thread.Sleep(50);
         }
      }
   }
}
