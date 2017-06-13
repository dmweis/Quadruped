using System;
using System.Collections.Generic;
using System.IO;
using dynamixel_sdk;

namespace DynamixelServo.Driver
{
   public class DynamixelDriver : IDisposable
   {
      // Control table address for protocol 1
      public const ushort ADDR_MX_ID = 3;
      public const ushort ADDR_MX_TORQUE_ENABLE = 24;
      public const ushort ADDR_MX_LED_ENABLE = 25;
      public const ushort ADDR_MX_GOAL_POSITION = 30;
      public const ushort ADDR_MX_MOVING_SPEED = 32;
      public const ushort ADDR_MX_PRESENT_POSITION = 36;
      public const ushort ADDR_MX_PRESENT_SPEED = 38;
      public const ushort ADDR_MX_PRESENT_LOAD = 40;
      public const ushort ADDR_MX_PRESENT_VOLTAGE = 42;
      public const ushort ADDR_MX_PRESENT_TEMP = 43;
      public const ushort ADDR_MX_PRESENT_MOVING = 46;

      // Control table address for protocol 2
      public const ushort ADDR_XL_ID = 3;
      public const ushort ADDR_XL_TORQUE_ENABLE = 24;
      public const ushort ADDR_XL_LED = 25;
      public const ushort ADDR_XL_GOAL_POSITION = 30;
      public const ushort ADDR_XL_GOAL_VELOCITY = 32;
      public const ushort ADDR_XL_PRESENT_POSITION = 37;
      public const ushort ADDR_XL_PRESENT_VELOCITY = 39;
      public const ushort ADDR_XL_PRESENT_LOAD = 41;
      public const ushort ADDR_XL_PRESENT_VOLTAGE = 45;
      public const ushort ADDR_XL_PRESENT_TEMPERATURE = 46;
      public const ushort ADDR_XL_PRESENT_MOVING = 49;

      private const int BaudRate = 1000000;
      private const int CommSuccess = 0;

      private readonly string _portName;
      private readonly int _portNumber;

      public DynamixelDriver(string portName)
      {
         _portName = portName;
         _portNumber = dynamixel.portHandler(_portName);
         dynamixel.packetHandler();


         if (!dynamixel.openPort(_portNumber))
         {
            throw new IOException($"Can not open port {_portName}");
         }
         if (!dynamixel.setBaudRate(_portNumber, BaudRate))
         {
            throw new IOException($"Can not set baud rate {BaudRate} on: {_portName}");
         }
      }

      public byte[] Search(byte startId = 0, byte endId = 252, DynamixelProtocol protocol = DynamixelProtocol.Version1)
      {
         List<byte> found = new List<byte>();
         for (byte i = startId; i < endId; i++)
         {
            if (Ping(i, protocol))
            {
               found.Add(i);
            }
         }
         return found.ToArray();
      }

      public void SetId(byte servoId, byte newServoId, DynamixelProtocol protocol = DynamixelProtocol.Version1)
      {
         if (Ping(newServoId, protocol))
         {
            throw new InvalidOperationException("New ID alredy taken");
         }
         ushort address = protocol == DynamixelProtocol.Version1 ? ADDR_MX_ID : ADDR_XL_ID;
         WriteByte(servoId, address, newServoId, protocol);
      }

      public bool IsMoving(byte servoId, DynamixelProtocol protocol = DynamixelProtocol.Version1)
      {
         ushort address = protocol == DynamixelProtocol.Version1 ? ADDR_MX_PRESENT_MOVING : ADDR_XL_PRESENT_MOVING;
         return ReadByte(servoId, address, protocol) > 0;
      }

      public void SetLed(byte servoId, bool on)
      {
         byte ledFlag = Convert.ToByte(on);
         WriteByte(servoId, ADDR_MX_LED_ENABLE, ledFlag, DynamixelProtocol.Version1);
      }

      public void SetLedColor(byte servoId, LedColor color)
      {
         WriteByte(servoId, ADDR_XL_LED, (byte)color, DynamixelProtocol.Version2);
      }

      public void SetTorque(byte servoId, bool on, DynamixelProtocol protocol = DynamixelProtocol.Version1)
      {
         byte torqueFlag = on ? (byte)1 : (byte)0;
         ushort address = protocol == DynamixelProtocol.Version1 ? ADDR_MX_TORQUE_ENABLE : ADDR_XL_TORQUE_ENABLE;
         WriteByte(servoId, address, torqueFlag, protocol);
      }

      public void SetGoalPosition(byte servoId, ushort goalPosition, DynamixelProtocol protocol = DynamixelProtocol.Version1)
      {
         ushort address = protocol == DynamixelProtocol.Version1 ? ADDR_MX_GOAL_POSITION : ADDR_XL_GOAL_POSITION;
         WriteUInt16(servoId, address, goalPosition, protocol);
      }

      public ushort GetGoalPosition(byte servoId, DynamixelProtocol protocol = DynamixelProtocol.Version1)
      {
         ushort address = protocol == DynamixelProtocol.Version1 ? ADDR_MX_GOAL_POSITION : ADDR_XL_GOAL_POSITION;
         return ReadUInt16(servoId, address, protocol);
      }

      public void SetMovingSpeed(byte servoId, ushort movingSpeed, DynamixelProtocol protocol = DynamixelProtocol.Version1)
      {
         ushort address = protocol == DynamixelProtocol.Version1 ? ADDR_MX_MOVING_SPEED : ADDR_XL_GOAL_VELOCITY;
         WriteUInt16(servoId, address, movingSpeed, protocol);
      }

      public ushort GetPresentPosition(byte servoId, DynamixelProtocol protocol = DynamixelProtocol.Version1)
      {
         ushort address = protocol == DynamixelProtocol.Version1 ? ADDR_MX_PRESENT_POSITION : ADDR_XL_PRESENT_POSITION;
         return ReadUInt16(servoId, address, protocol);
      }

      public ushort GetPresentSpeed(byte servoId, DynamixelProtocol protocol = DynamixelProtocol.Version1)
      {
         ushort address = protocol == DynamixelProtocol.Version1 ? ADDR_MX_PRESENT_SPEED : ADDR_XL_PRESENT_VELOCITY;
         return ReadUInt16(servoId, address, protocol);
      }

      public ushort GetPresentLoad(byte servoId, DynamixelProtocol protocol = DynamixelProtocol.Version1)
      {
         ushort address = protocol == DynamixelProtocol.Version1 ? ADDR_MX_PRESENT_LOAD : ADDR_XL_PRESENT_LOAD;
         return ReadUInt16(servoId, address, protocol);
      }

      public byte GetTemperature(byte servoId, DynamixelProtocol protocol = DynamixelProtocol.Version1)
      {
         ushort address = protocol == DynamixelProtocol.Version1 ? ADDR_MX_PRESENT_TEMP : ADDR_XL_PRESENT_TEMPERATURE;
         return ReadByte(servoId, address, protocol);
      }

      public byte GetVoltage(byte servoId, DynamixelProtocol protocol = DynamixelProtocol.Version1)
      {
         ushort address = protocol == DynamixelProtocol.Version1 ? ADDR_MX_PRESENT_VOLTAGE : ADDR_XL_PRESENT_VOLTAGE;
         return ReadByte(servoId, address, protocol);
      }

      public ushort GetModelNumber(byte servoId, DynamixelProtocol protocol = DynamixelProtocol.Version1)
      {
         ushort modelNumber = dynamixel.pingGetModelNum(_portNumber, (int)protocol, servoId);
         VerifyLastMessage(protocol);
         return modelNumber;
      }

      public bool Ping(byte servoId, DynamixelProtocol protocol = DynamixelProtocol.Version1)
      {
         dynamixel.ping(_portNumber, (int)protocol, servoId);
         if (dynamixel.getLastTxRxResult(_portNumber, (int)protocol) != CommSuccess)
         {
            return false;
         }
         byte dxlError = dynamixel.getLastRxPacketError(_portNumber, (int)protocol);
         if (dxlError != 0)
         {
            throw new IOException(DynamixelErrorHelper.GetRxPackErrorDescription(dxlError));
         }
         return true;
      }

      private void WriteByte(byte servoId, ushort address, byte data, DynamixelProtocol protocol)
      {
         dynamixel.write1ByteTxRx(_portNumber, (int)protocol, servoId, address, data);
         VerifyLastMessage(protocol);
      }

      private byte ReadByte(byte servoId, ushort address, DynamixelProtocol protocol)
      {
         byte incoming = dynamixel.read1ByteTxRx(_portNumber, (int)protocol, servoId, address);
         VerifyLastMessage(protocol);
         return incoming;
      }

      private void WriteUInt16(byte servoId, ushort address, ushort data, DynamixelProtocol protocol)
      {
         dynamixel.write2ByteTxRx(_portNumber, (int)protocol, servoId, address, data);
         VerifyLastMessage(protocol);
      }

      private ushort ReadUInt16(byte servoId, ushort address, DynamixelProtocol protocol)
      {
         ushort incoming = dynamixel.read2ByteTxRx(_portNumber, (int)protocol, servoId, address);
         VerifyLastMessage(protocol);
         return incoming;
      }

      private void VerifyLastMessage(DynamixelProtocol protocol)
      {
         int commResult = dynamixel.getLastTxRxResult(_portNumber, (int)protocol);
         if (commResult != CommSuccess)
         {
            throw new IOException(DynamixelErrorHelper.GetTxRxResultDescription(commResult));
         }
         byte dxlError = dynamixel.getLastRxPacketError(_portNumber, (int)protocol);
         if (dxlError != 0)
         {
            throw new IOException(DynamixelErrorHelper.GetRxPackErrorDescription(dxlError));
         }
      }

      public void Dispose()
      {
         dynamixel.closePort(_portNumber);
      }
   }

   public enum DynamixelProtocol
   {
      Version1 = 1,
      Version2 = 2
   }

   public enum LedColor
   {
      White = 0b111,
      Pink = 0b101,
      BlueGreen = 0b110,
      Yellow = 0b11,
      Blue = 0b100,
      Green = 0b010,
      Red = 0b1
   }
}
