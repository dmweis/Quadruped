namespace DynamixelServo.Driver
{
    public class ServoTelemetrics
    {
        public int Id { get; set; }
        public int Temperature { get; set; }
        public float Voltage { get; set; }
        public int Load { get; set; }


        public override string ToString()
        {
            return $"Id: {Id} Temperature: {Temperature}°C Voltage: {Voltage}V Load: {Load:000}";
        }
    }
}
