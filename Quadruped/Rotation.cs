using System.Numerics;

namespace Quadruped
{
    public class Rotation
    {
        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public float Roll { get; set; }

        public Rotation(float yaw, float pitch, float roll)
        {
            Yaw = yaw;
            Pitch = pitch;
            Roll = roll;
        }

        public Rotation()
        {
            
        }

        public bool Similar(Rotation other, float marginOfError = float.Epsilon)
        {
            return Vector3.Distance(new Vector3(Yaw, Pitch, Roll), new Vector3(other.Yaw, other.Pitch, other.Roll)) <= marginOfError;
        }
    }
}
