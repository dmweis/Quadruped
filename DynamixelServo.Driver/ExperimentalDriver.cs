using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace DynamixelServo.Driver
{
   public class ExperimentalDriver
   {

      public static void WriteMessageAndCheck(string portName, byte id)
      {
         using (SerialPort port = new SerialPort(portName, 1000000))
         {
            port.Open();
            Thread.Sleep(400);
            byte instruction = 3; // write
            byte parameter1 = 25; // address of LED
            byte parameter2 = 0; // turn off LED

             while (true)
             {
                byte[] data = EncodeMessage(id, instruction, new[] { parameter1, parameter2 });
                port.Write(data, 0, data.Length);
                // sent
                List<byte> buffer = new List<byte>();
                while (true)
                {
                   buffer.Add((byte)port.ReadByte());
                   bool flag = DecodeMessage(buffer.ToArray());
                   if (flag)
                   {
                      buffer.Clear();
                       System.Diagnostics.Debug.WriteLine("Success!");
                      break;
                   }
                }
                 
             }
         }
      }

      public static byte[] EncodeMessage(byte id, byte instruction, byte[] parameters)
      {
         byte length = (byte)(2 + parameters.Length);
         int checksum = id + length + instruction + parameters.Sum(num => num);
         checksum = ~checksum;
         byte[] data = new byte[parameters.Length + 6];
         data[0] = 0xFF;
         data[1] = 0xFF;
         data[2] = id;
         data[3] = length;
         data[4] = instruction;
         for (int i = 0; i < parameters.Length; i++)
         {
            data[5 + i] = parameters[i];
         }
         data[data.Length - 1] = (byte)checksum;
         return data;
      }

      public static bool DecodeMessage(byte[] data)
      {
          try
          {
         if (data[0] != 0xFF || data[1] != 0xFF)
         {
            // not a packet
            return false;
         }
         byte id = data[2];
         byte length = data[3];
         byte error = data[4];
         byte[] incoming = new byte[length - 2];
         Array.Copy(data, 5, incoming, 0, length - 2);
         byte checksum = data[3 + length];
         int localChecksum = id + length + error + incoming.Sum(num => num);
         localChecksum = ~localChecksum;
         if (checksum != (byte)localChecksum)
         {
            throw new IOException("Checksum error");
         }
         // Success!
         return true;

          }
          catch (IndexOutOfRangeException)
          {
              return false;
          }
      }
   }
}
