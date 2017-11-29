using System;
using System.Numerics;

namespace Quadruped.WebInterface.RTC
{
    public class RobotMessage
    {
        public JoystickType JoystickType { get; set; }
        public float Angle { get; set; }
        public MessageType MessageType { get; set; }
        public float Distance { get; set; }

        public float GetScaledX(float deadzone, float min, float max)
        {
            if (MessageType != MessageType.Movement)
            {
                return 0;
            }
            float x = (float) Math.Cos(Angle) * Distance / 50;
            if (Math.Abs(x) < deadzone)
            {
                return 0;
            }
            if (x > 0)
            {
                x = (x - deadzone) * (max - min) / (1 - deadzone) + min;
            }
            else
            {
                x = (x + deadzone) * (-max + min) / (-1 + deadzone) - min;
            }
            return x;
        }

        public Vector2 CalculateHeadingVector(float deadzone = 0.0f)
        {
            if (MessageType != MessageType.Movement)
            {
                return Vector2.Zero;
            }
            if (Math.Abs(Distance / 50) < deadzone)
            {
                return Vector2.Zero;
            }
            float y = -(float)Math.Cos(Angle);
            float x = (float)Math.Sin(Angle);
            var vector = new Vector2
            {
                X = x,
                Y = y
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
        Rotation,
        Camera,
        Translation,
        Height,
        BodyRotation
    }

    public enum MessageType
    {
        Movement = 0,
        Stop = 1,
        Start = 2,
        Reset = 4
    }
}
