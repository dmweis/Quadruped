using System;
using System.Numerics;

namespace DynamixelServo.Quadruped
{
    public static class VectorExtensions
    {
        public static Vector3 Normal(this Vector3 vector)
        {
            return Vector3.Normalize(vector);
        }

        public static bool Similar(this Vector3 a, Vector3 b, float marginOfError = float.Epsilon)
        {
            return Vector3.Distance(a, b) <= marginOfError;
        }

        public static Vector3 MoveTowards(this Vector3 current, Vector3 target, float distance)
        {
            var transport = target - current;
            var len = transport.Length();
            if (len < distance)
            {
                return target;
            }
            return current + transport.Normal() * distance;
        }

        public static float DistanceSquared(this Vector3 current, Vector3 target)
        {
            return Vector3.DistanceSquared(current, target);
        }

        public static Vector2 ToDirectionVector2(this Vector3 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        public static Vector2 Normal(this Vector2 vector)
        {
            return Vector2.Normalize(vector);
        }

        public static Vector2 Rotate(this Vector2 vector, Angle angle)
        {
            Vector2 rotatedVector = new Vector2
            {
                X = (float)(vector.X * Math.Cos(angle.DegreeToRad()) - vector.Y * Math.Sin(angle.DegreeToRad())),
                Y = (float)(vector.X * Math.Sin(angle.DegreeToRad()) + vector.Y * Math.Cos(angle.DegreeToRad()))
            };
            return rotatedVector;
        }
    }
}
