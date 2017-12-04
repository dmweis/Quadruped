using System.Numerics;

namespace Quadruped
{
    public class Vector3Wrapper
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public static explicit operator Vector3(Vector3Wrapper original)
        {
            return new Vector3(original.X, original.Y, original.Z);
        }

        public override string ToString()
        {
            return ((Vector3) this).ToString();
        }
    }
}
