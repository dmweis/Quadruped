namespace DynamixelServo.Quadruped.WebInterface.VideoStreaming
{
    public class StreamerConfig
    {
        public int ImageQuality { get; set; } = 85;
        public int HorizontalResolution { get; set; } = 800;
        public int VerticalResolution { get; set; } = 600;
        public int Framerate { get; set; } = 10;
    }
}
