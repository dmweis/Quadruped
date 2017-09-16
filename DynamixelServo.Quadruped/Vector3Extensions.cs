using System.Numerics;

namespace DynamixelServo.Quadruped
{
    public static class Vector3Extensions
    {
        public static Vector3 Normal(this Vector3 vector)
        {
            float len = vector.Length();
            return new Vector3(vector.X / len, vector.Y / len, vector.Z / len);
        }

        public static bool Similar(this Vector3 a, Vector3 b, float marginOfError = float.Epsilon)
        {
            return Vector3.Distance(a, b) <= marginOfError;
        }
    }
}
