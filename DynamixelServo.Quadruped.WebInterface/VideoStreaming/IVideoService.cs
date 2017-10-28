namespace DynamixelServo.Quadruped.WebInterface.VideoStreaming
{
    public interface IVideoService
    {
        string Port { get; }
        bool StreamRunning { get; }
    }
}
