using System.Threading.Tasks;

namespace Quadruped.WebInterface.VideoStreaming
{
    public interface IVideoService
    {
        bool StreamRunning { get; }
        StreamerConfig StreamerConfiguration { get; set; }
        Task RestartAsync();
        Task EnsureStreamOn();
    }
}
