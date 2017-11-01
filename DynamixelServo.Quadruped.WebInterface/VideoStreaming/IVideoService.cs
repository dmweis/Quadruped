using System.Threading.Tasks;

namespace DynamixelServo.Quadruped.WebInterface.VideoStreaming
{
    public interface IVideoService
    {
        bool StreamRunning { get; }
        StreamerConfig StreamerConfiguration { get; }
        Task RestartAsync(StreamerConfig config);
    }
}
