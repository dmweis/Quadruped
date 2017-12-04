namespace Quadruped.WebInterface.IMU
{
    public class ImuData
    {
        public Vector3Wrapper Acceleration { get; set; }
        public Vector3Wrapper Magnetic { get; set; }
        public Vector3Wrapper Gyroscopic { get; set; }
        public float Temperature { get; set; }

        public override string ToString()
        {
            return $"Acceleration {Acceleration}\nMagnetic {Magnetic}\nGyroscopic {Gyroscopic}\nTemperature {Temperature}";
        }
    }
}
