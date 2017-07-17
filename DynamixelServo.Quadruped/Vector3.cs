using System;

namespace DynamixelServo.Quadruped
{
    public struct Vector3
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3 operator *(Vector3 vector, float multiplier)
        {
            return new Vector3(vector.X * multiplier, vector.Y * multiplier, vector.Z * multiplier);
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static float Distance(Vector3 a, Vector3 b)
        {
            return (float) Math.Sqrt((a.X - b.X).Square() + (a.Y - b.Y).Square() + (a.Z - b.Z).Square());
        }

        public override string ToString()
        {
            return $"[{X:f3}; {Y:f3}; {Z:f3}]";
        }
    }
}
