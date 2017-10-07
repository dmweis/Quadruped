using System.Numerics;

namespace DynamixelServo.Quadruped
{
    public static class Vector3Extensions
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

        public static Vector2 ToGroundProfile(this Vector3 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        public static Vector2 Normal(this Vector2 vector)
        {
            return Vector2.Normalize(vector);
        }
    }
}
