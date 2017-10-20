using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace dynamixelServo.Quadruped.WebInterface.RTC
{
    public class RobotMessage
    {
        public JoystickType JoystickType { get; set; }
        public float Angle { get; set; }
        public MessageType MessageType { get; set; }
        public float Force { get; set; }

        public Vector2 CalculateHeadingVector(float deadzone = 0.0f)
        {
            if (MessageType != MessageType.Movement)
            {
                return Vector2.Zero;
            }
            if (Force < deadzone)
            {
                return Vector2.Zero;
            }
            float x = (float)Math.Cos(Angle);
            float y = (float)Math.Sin(Angle);
            var vector = new Vector2
            {
                X = Math.Abs(x) > deadzone ? x : 0f,
                Y = Math.Abs(y) > deadzone ? y : 0f
            };
            return vector;
        }

        public override string ToString()
        {
            if (MessageType == MessageType.Movement)
            {
                return $"Type: {JoystickType} Heading: {CalculateHeadingVector(0.2f)} Angle: {Angle} radian";
            }
            return $"Type: {JoystickType} Action: {MessageType}";
        }
    }

    public enum JoystickType
    {
        Direction,
        Rotation
    }

    public enum MessageType
    {
        Movement,
        Stop,
        Start
    }
}
