using System.Threading.Tasks;

namespace DynamixelServo.Quadruped.WebInterface.VideoStreaming
{
    public interface IVideoService
    {
        bool StreamRunning { get; }
        StreamerConfig StreamerConfiguration { get; set; }
        Task RestartAsync();
        Task EnsureStreamOn();
    }
}
