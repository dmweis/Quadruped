namespace DynamixelServo.Quadruped.WebInterface.VideoStreaming
{
    public class MockVideoStream : IVideoService
    {
        public bool StreamRunning => false;
    }
}
