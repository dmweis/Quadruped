using System;
using System.Collections.Generic;
using System.IO;
using dynamixel_sdk;

namespace DynamixelServo.Driver
{
   public class DynamixelDriver : IDisposable
   {
      // Control table address
      public const ushort ADDR_TORQUE_ENABLE = 24;
      public const ushort ADDR_LED_ENABLE = 25;
      public const ushort ADDR_GOAL_POSITION = 30;
      public const ushort ADDR_MOVING_SPEED = 32;
      public const ushort ADDR_PRESENT_POSITION = 36;
      public const ushort ADDR_PRESENT_SPEED = 38;
      public const ushort ADDR_PRESENT_LOAD = 40;
      public const ushort ADDR_PRESENT_VOLTAGE = 42;
      public const ushort ADDR_PRESENT_TEMP = 43;
      public const ushort ADDR_PRESENT_MOVING = 46;

      private const int ProtocolVersion = 1;
      private const int BaudRate = 1000000;
      private const int CommFail = -1001;
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

      public byte[] Search(byte startId = 0, byte endId = 252)
      {
         List<byte> found = new List<byte>();
         for (byte i = startId; i < endId; i++)
         {
            if (Ping(i))
            {
               found.Add(i);
            }
         }
         return found.ToArray();
      }

      public bool IsMoving(byte servoId)
      {
         return ReadByte(servoId, ADDR_PRESENT_MOVING) > 0;
      }

      public void SetLed(byte servoId, bool on)
      {
         byte ledFlag = on ? (byte)1 : (byte)0;
         WriteByte(servoId, ADDR_LED_ENABLE, ledFlag);
      }

      public void SetTorque(byte servoId, bool on)
      {
         byte torqueFlag = on ? (byte)1 : (byte)0;
         WriteByte(servoId, ADDR_TORQUE_ENABLE, torqueFlag);
      }

      public void WriteGoalPosition(byte servoId, ushort goalPosition)
      {
         WriteUInt16(servoId, ADDR_GOAL_POSITION, goalPosition);
      }

      public void WriteMovingSpeed(byte servoId, ushort movingSpeed)
      {
         WriteUInt16(servoId, ADDR_MOVING_SPEED, movingSpeed);
      }

      public ushort ReadPresentPosition(byte servoId)
      {
         return ReadUInt16(servoId, ADDR_PRESENT_POSITION);
      }

      public ushort ReadPresentSpeed(byte servoId)
      {
         return ReadUInt16(servoId, ADDR_PRESENT_SPEED);
      }

      public ushort ReadPresentLoad(byte servoId)
      {
         return ReadUInt16(servoId, ADDR_PRESENT_LOAD);
      }

      public byte ReadTemperature(byte servoId)
      {
         return ReadByte(servoId, ADDR_PRESENT_TEMP);
      }

      public byte ReadVoltage(byte servoId)
      {
         return ReadByte(servoId, ADDR_PRESENT_VOLTAGE);
      }

      public ushort GetModelNumber(byte servoId)
      {
         ushort modelNumber = dynamixel.pingGetModelNum(_portNumber, ProtocolVersion, servoId);
         VerifyLastMessage();
         return modelNumber;
      }

      public bool Ping(byte servoId)
      {
         dynamixel.ping(_portNumber, ProtocolVersion, servoId);
         byte dxlError = 0;
         if (dynamixel.getLastTxRxResult(_portNumber, ProtocolVersion) != CommSuccess)
         {
            return false;
         }
         if ((dxlError = dynamixel.getLastRxPacketError(_portNumber, ProtocolVersion)) != 0)
         {
            throw new IOException(DynamixelErrorHelper.GetRxPackErrorDescription(dxlError));
         }
         return true;
      }

      private void WriteByte(byte servoId, ushort address, byte data)
      {
         dynamixel.write1ByteTxRx(_portNumber, ProtocolVersion, servoId, address, data);
         VerifyLastMessage();
      }

      private byte ReadByte(byte servoId, ushort address)
      {
         byte incoming = dynamixel.read1ByteTxRx(_portNumber, ProtocolVersion, servoId, address);
         VerifyLastMessage();
         return incoming;
      }

      private void WriteUInt16(byte servoId, ushort address, ushort data)
      {
         dynamixel.write2ByteTxRx(_portNumber, ProtocolVersion, servoId, address, data);
         VerifyLastMessage();
      }

      private ushort ReadUInt16(byte servoId, ushort address)
      {
         ushort incoming = dynamixel.read2ByteTxRx(_portNumber, ProtocolVersion, servoId, address);
         VerifyLastMessage();
         return incoming;
      }

      private void VerifyLastMessage()
      {
         byte dxlError = 0;
         int commResult = CommFail;
         if ((commResult = dynamixel.getLastTxRxResult(_portNumber, ProtocolVersion)) != CommSuccess)
         {
            throw new IOException(DynamixelErrorHelper.GetTxRxResultDescription(commResult));
         }
         if ((dxlError = dynamixel.getLastRxPacketError(_portNumber, ProtocolVersion)) != 0)
         {
            throw new IOException(DynamixelErrorHelper.GetRxPackErrorDescription(dxlError));
         }
      }

      public void Dispose()
      {
         dynamixel.closePort(_portNumber);
      }
   }
}
