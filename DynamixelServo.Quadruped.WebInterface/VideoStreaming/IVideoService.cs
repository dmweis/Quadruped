namespace DynamixelServo.Quadruped.WebInterface.VideoStreaming
{
    public interface IVideoService
    {
        string StreamPath { get; }
        bool StreamRunning { get; }
    }
}
