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

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Vector3 operator -(Vector3 a, Vector2 b)
        {
            return new Vector3(a.X - b.X, a.Y - b.Y, a.Z);
        }

        public static explicit operator Vector3(Vector2 vector)
        {
            return new Vector3(vector.X, vector.Y, 0);
        }
    }
}
