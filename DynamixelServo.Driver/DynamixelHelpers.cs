using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DynamixelServo.Driver
{
   public static class DynamixelHelpers
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

      public static void SwitchId(byte servo1Id, byte servo2Id, DynamixelDriver driver, DynamixelProtocol protocol = DynamixelProtocol.Version1)
      {
         // find empty index
         byte freeIndex = 255;
         for (byte index = 1; index < 252; index++)
         {
            if (!driver.Ping(index, protocol))
            {
               freeIndex = index;
               break;
            }
         }
         if (freeIndex == 255)
         {
            throw new InvalidOperationException("Can't find free ID");
         }
         driver.SetId(servo1Id, freeIndex, protocol);
         driver.SetId(servo2Id, servo1Id, protocol);
         driver.SetId(freeIndex, servo2Id, protocol);
      }

      public static void MoveToBlocking(byte servoId, ushort goal, DynamixelDriver driver, DynamixelProtocol protocol = DynamixelProtocol.Version1)
      {
         driver.SetGoalPosition(servoId, goal);
         while (driver.IsMoving(servoId, protocol))
         {
            Thread.Sleep(50);
         }
      }

      public static void MoveToAll(byte[] servoIds, ushort[] goals, DynamixelDriver driver, DynamixelProtocol protocol = DynamixelProtocol.Version1)
      {
         if (servoIds.Length != goals.Length)
         {
            throw new ArgumentException($"{nameof(servoIds)} and {nameof(goals)} have to be the same length");
         }
         for (int index = 0; index < servoIds.Length; index++)
         {
            driver.SetGoalPosition(servoIds[index], goals[index], protocol);
         }
      }

      public static void MoveToAllBlocking(byte[] servoIds, ushort[] goals, DynamixelDriver driver, DynamixelProtocol protocol = DynamixelProtocol.Version1)
      {
         if (servoIds.Length != goals.Length)
         {
            throw new ArgumentException($"{nameof(servoIds)} and {nameof(goals)} have to be the same length");
         }
         for (int index = 0; index < servoIds.Length; index++)
         {
            driver.SetGoalPosition(servoIds[index], goals[index], protocol);
         }
         while (!servoIds.All(index => !driver.IsMoving(index, protocol)))
         {
            Thread.Sleep(50);
         }
      }
   }
}
