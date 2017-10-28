namespace DynamixelServo.Quadruped.WebInterface.VideoStreaming
{
    public class MockVideoStream : IVideoService
    {
        public string Port => 8080.ToString();
        public bool StreamRunning => true;
    }
}
