using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using dynamixel_sdk;
using System.Linq;

namespace DynamixelServo.Driver
{
    public class DynamixelDriver : IDisposable
    {
        // Control table address for protocol 1
        public const ushort ADDR_MX_ID = 3;
        public const ushort ADDR_MX_CW_ANGLE_LIMIT = 6;
        public const ushort ADDR_MX_CCW_ANGLE_LIMIT = 8;
        public const ushort ADDR_MX_TORQUE_ENABLE = 24;
        public const ushort ADDR_MX_LED_ENABLE = 25;
        public const ushort ADDR_MX_CW_COMPLIANCE_MARGIN = 26;
        public const ushort ADDR_MX_CCW_COMPLIANCE_MARGIN = 27;
        public const ushort ADDR_MX_CW_COMPLIANCE_SLOPE = 28;
        public const ushort ADDR_MX_CCW_COMPLIANCE_SLOPE = 29;
        public const ushort ADDR_MX_GOAL_POSITION = 30;
        public const ushort ADDR_MX_MOVING_SPEED = 32;
        public const ushort ADDR_MX_TORQUE_LIMIT = 34;
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
        public const ushort ADDR_XL_GOAL_TORQUE = 35;
        public const ushort ADDR_XL_PRESENT_POSITION = 37;
        public const ushort ADDR_XL_PRESENT_VELOCITY = 39;
        public const ushort ADDR_XL_PRESENT_LOAD = 41;
        public const ushort ADDR_XL_PRESENT_VOLTAGE = 45;
        public const ushort ADDR_XL_PRESENT_TEMPERATURE = 46;
        public const ushort ADDR_XL_PRESENT_MOVING = 49;

        private const int DefaultBaudRate = 1000000;
        private const int CommSuccess = 0;

        private readonly string _portName;
        private readonly int _portNumber;

        public DynamixelDriver(string portName, int baudRate = DefaultBaudRate)
        {
            _portName = portName;
            _portNumber = dynamixel.portHandler(_portName);
            dynamixel.packetHandler();


            if (!dynamixel.openPort(_portNumber))
            {
                throw new IOException($"Can not open port {_portName}");
            }
            if (!dynamixel.setBaudRate(_portNumber, baudRate))
            {
                throw new IOException($"Can not set baud rate {baudRate} on: {_portName}");
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

        public void SetComplianceMargin(byte servoId, byte complianceMargin, DynamixelProtocol protocol = DynamixelProtocol.Version1)
        {
            SetClockwiseComplianceMargin(servoId, complianceMargin, protocol);
            SetCounterClockwiseComplianceMargin(servoId, complianceMargin, protocol);
        }

        public void SetClockwiseComplianceMargin(byte servoId, byte complianceMargin, DynamixelProtocol protocol = DynamixelProtocol.Version1)
        {
            if (protocol != DynamixelProtocol.Version1)
            {
                throw new NotImplementedException();
            }
            WriteByte(servoId, ADDR_MX_CW_COMPLIANCE_MARGIN, complianceMargin, protocol);
        }

        public void SetCounterClockwiseComplianceMargin(byte servoId, byte complianceMargin, DynamixelProtocol protocol = DynamixelProtocol.Version1)
        {
            if (protocol != DynamixelProtocol.Version1)
            {
                throw new NotImplementedException();
            }
            WriteByte(servoId, ADDR_MX_CCW_COMPLIANCE_MARGIN, complianceMargin, protocol);
        }

        public void SetComplianceSlope(byte servoId, ComplianceSlope complianceSlope, DynamixelProtocol protocol = DynamixelProtocol.Version1)
        {
            SetClockwiseComplianceSlope(servoId, complianceSlope, protocol);
            SetCounterClockwiseComplianceSlope(servoId, complianceSlope, protocol);
        }

        public void SetClockwiseComplianceSlope(byte servoId, ComplianceSlope complianceSlope, DynamixelProtocol protocol = DynamixelProtocol.Version1)
        {
            if (protocol != DynamixelProtocol.Version1)
            {
                throw new NotImplementedException();
            }
            WriteByte(servoId, ADDR_MX_CW_COMPLIANCE_SLOPE, (byte)complianceSlope, protocol);
        }

        public void SetCounterClockwiseComplianceSlope(byte servoId, ComplianceSlope complianceSlope, DynamixelProtocol protocol = DynamixelProtocol.Version1)
        {
            if (protocol != DynamixelProtocol.Version1)
            {
                throw new NotImplementedException();
            }
            WriteByte(servoId, ADDR_MX_CCW_COMPLIANCE_SLOPE, (byte)complianceSlope, protocol);
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

        public void SetGoalPositionInDegrees(byte servoId, float angle, DynamixelProtocol protocol = DynamixelProtocol.Version1)
        {
            SetGoalPosition(servoId, DegreesToUnits(angle), protocol);
        }

        public void SetGoalPosition(byte servoId, ushort goalPosition, DynamixelProtocol protocol = DynamixelProtocol.Version1)
        {
            ushort address = protocol == DynamixelProtocol.Version1 ? ADDR_MX_GOAL_POSITION : ADDR_XL_GOAL_POSITION;
            WriteUInt16(servoId, address, goalPosition, protocol);
        }

        public float GetGoalPositionInDegrees(byte servoId, DynamixelProtocol protocol = DynamixelProtocol.Version1)
        {
            return UnitsToDegrees(GetGoalPosition(servoId, protocol));
        }

        public ushort GetGoalPosition(byte servoId, DynamixelProtocol protocol = DynamixelProtocol.Version1)
        {
            ushort address = protocol == DynamixelProtocol.Version1 ? ADDR_MX_GOAL_POSITION : ADDR_XL_GOAL_POSITION;
            return ReadUInt16(servoId, address, protocol);
        }

        public void MoveToBlocking(byte servoId, ushort goal, DynamixelProtocol protocol = DynamixelProtocol.Version1, int querryDelay = 50)
        {
            SetGoalPosition(servoId, goal);
            while (IsMoving(servoId, protocol))
            {
                Thread.Sleep(querryDelay);
            }
        }

        public void MoveToAll(byte[] servoIds, ushort[] goals, DynamixelProtocol protocol = DynamixelProtocol.Version1)
        {
            if (servoIds.Length != goals.Length)
            {
                throw new ArgumentException($"{nameof(servoIds)} and {nameof(goals)} have to be the same length");
            }
            for (int index = 0; index < servoIds.Length; index++)
            {
                SetGoalPosition(servoIds[index], goals[index], protocol);
            }
        }

        public void MoveToAllBlocking(byte[] servoIds, ushort[] goals, DynamixelProtocol protocol = DynamixelProtocol.Version1, int querryDelay = 50)
        {
            if (servoIds.Length != goals.Length)
            {
                throw new ArgumentException($"{nameof(servoIds)} and {nameof(goals)} have to be the same length");
            }
            for (int index = 0; index < servoIds.Length; index++)
            {
                SetGoalPosition(servoIds[index], goals[index], protocol);
            }
            while (!servoIds.All(index => !IsMoving(index, protocol)))
            {
                Thread.Sleep(querryDelay);
            }
        }

        public void SetMovingSpeed(byte servoId, ushort movingSpeed, bool cw = false, DynamixelProtocol protocol = DynamixelProtocol.Version1)
        {
            if (movingSpeed > 1023)
            {
                throw new ArgumentOutOfRangeException($"{nameof(movingSpeed)} is too high. Max value is 1023");
            }
            if (movingSpeed > 0 && cw) // if speed zero and bit for CW set true servo mode won't work
            {
                movingSpeed |= 1 << 10; // set direction bit high
            }
            ushort address = protocol == DynamixelProtocol.Version1 ? ADDR_MX_MOVING_SPEED : ADDR_XL_GOAL_VELOCITY;
            WriteUInt16(servoId, address, movingSpeed, protocol);
        }

        public void SetTorqueGoal(byte servoId, ushort torque, DynamixelProtocol protocol = DynamixelProtocol.Version1)
        {
            ushort address = protocol == DynamixelProtocol.Version1 ? ADDR_MX_TORQUE_LIMIT : ADDR_XL_GOAL_TORQUE;
            WriteUInt16(servoId, address, torque, protocol);
        }

        public float GetPresentPositionInDegrees(byte servoId, DynamixelProtocol protocol = DynamixelProtocol.Version1)
        {
            return UnitsToDegrees(GetPresentPosition(servoId, protocol));
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

        public int GetPresentLoad(byte servoId, DynamixelProtocol protocol = DynamixelProtocol.Version1)
        {
            ushort address = protocol == DynamixelProtocol.Version1 ? ADDR_MX_PRESENT_LOAD : ADDR_XL_PRESENT_LOAD;
            ushort loadData = ReadUInt16(servoId, address, protocol);
            bool ccw = Convert.ToBoolean(loadData & 1 << 10);
            int load = (loadData & ~(1 << 10)) / 10;
            load = ccw ? -load : load;
            return load;
        }

        public byte GetTemperature(byte servoId, DynamixelProtocol protocol = DynamixelProtocol.Version1)
        {
            ushort address = protocol == DynamixelProtocol.Version1 ? ADDR_MX_PRESENT_TEMP : ADDR_XL_PRESENT_TEMPERATURE;
            return ReadByte(servoId, address, protocol);
        }

        public float GetVoltage(byte servoId, DynamixelProtocol protocol = DynamixelProtocol.Version1)
        {
            ushort address = protocol == DynamixelProtocol.Version1 ? ADDR_MX_PRESENT_VOLTAGE : ADDR_XL_PRESENT_VOLTAGE;
            return ReadByte(servoId, address, protocol) / 10f;
        }

        public ushort GetModelNumber(byte servoId, DynamixelProtocol protocol = DynamixelProtocol.Version1)
        {
            ushort modelNumber = dynamixel.pingGetModelNum(_portNumber, (int)protocol, servoId);
            VerifyLastMessage(servoId, protocol);
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

        public ServoTelemetrics GetTelemetrics(byte servo, DynamixelProtocol protocol = DynamixelProtocol.Version1)
        {
            ServoTelemetrics telemetrics = new ServoTelemetrics
            {
                Id = servo,
                Voltage = GetVoltage(servo, protocol),
                Temperature = GetTemperature(servo, protocol),
                Load = GetPresentLoad(servo, protocol)
            };
            return telemetrics;
        }

        public void SetCcwMaxAngleLimit(byte servoId, ushort maxAngle, DynamixelProtocol protocol = DynamixelProtocol.Version1)
        {
            WriteUInt16(servoId, ADDR_MX_CCW_ANGLE_LIMIT, maxAngle, protocol);
        }

        public void SetCwMinAngleLimit(byte servoId, ushort minAngle, DynamixelProtocol protocol = DynamixelProtocol.Version1)
        {
            WriteUInt16(servoId, ADDR_MX_CW_ANGLE_LIMIT, minAngle, protocol);
        }

        public void SetWheelMode(byte servoId, DynamixelProtocol protocol = DynamixelProtocol.Version1)
        {
            if (protocol != DynamixelProtocol.Version1)
            {
                throw new NotImplementedException("Support for protocol 1 only");
            }
            WriteUInt16(servoId, ADDR_MX_CW_ANGLE_LIMIT, 0, protocol);
            WriteUInt16(servoId, ADDR_MX_CCW_ANGLE_LIMIT, 0, protocol);
        }

        public void SetServoMode(byte servoId, DynamixelProtocol protocol = DynamixelProtocol.Version1)
        {
            if (protocol != DynamixelProtocol.Version1)
            {
                throw new NotImplementedException("Support for protocol 1 only");
            }
            WriteUInt16(servoId, ADDR_MX_CW_ANGLE_LIMIT, 0, protocol);
            WriteUInt16(servoId, ADDR_MX_CCW_ANGLE_LIMIT, 1023, protocol);
        }

        private void WriteByte(byte servoId, ushort address, byte data, DynamixelProtocol protocol)
        {
            dynamixel.write1ByteTxRx(_portNumber, (int)protocol, servoId, address, data);
            VerifyLastMessage(servoId, protocol);
        }

        private byte ReadByte(byte servoId, ushort address, DynamixelProtocol protocol)
        {
            byte incoming = dynamixel.read1ByteTxRx(_portNumber, (int)protocol, servoId, address);
            VerifyLastMessage(servoId, protocol);
            return incoming;
        }

        private void WriteUInt16(byte servoId, ushort address, ushort data, DynamixelProtocol protocol)
        {
            dynamixel.write2ByteTxRx(_portNumber, (int)protocol, servoId, address, data);
            VerifyLastMessage(servoId, protocol);
        }

        private ushort ReadUInt16(byte servoId, ushort address, DynamixelProtocol protocol)
        {
            ushort incoming = dynamixel.read2ByteTxRx(_portNumber, (int)protocol, servoId, address);
            VerifyLastMessage(servoId, protocol);
            return incoming;
        }

        private void VerifyLastMessage(byte servoId, DynamixelProtocol protocol)
        {
            int commResult = dynamixel.getLastTxRxResult(_portNumber, (int)protocol);
            if (commResult != CommSuccess)
            {
                throw new IOException(DynamixelErrorHelper.GetTxRxResultDescription(commResult) + $" on servo: {servoId}");
            }
            byte dxlError = dynamixel.getLastRxPacketError(_portNumber, (int)protocol);
            if (dxlError != 0)
            {
                throw new IOException(DynamixelErrorHelper.GetRxPackErrorDescription(dxlError) + $" on servo: {servoId}");
            }
        }

        public static float UnitsToDegrees(ushort units)
        {
            return Constrain(MapFloat(units, 0, 1023, 0, 300), 0, 300);
        }

        public static ushort DegreesToUnits(float degrees)
        {
            return (ushort)Math.Round(Constrain(MapFloat(degrees, 0, 300, 0, 1023), 0, 1023));
        }

        private static float MapFloat(float value, float inMin, float inMax, float outMin, float outMax)
        {
            return (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }

        private static float Constrain(float value, float min, float max)
        {
            float tempMin = Math.Min(min, max);
            max = Math.Max(min, max);
            min = tempMin;
            value = Math.Max(value, min);
            return Math.Min(value, max);
        }

        public void Dispose()
        {
            dynamixel.closePort(_portNumber);
        }
    }
}
