using System.Threading.Tasks;

namespace DynamixelServo.Quadruped.WebInterface.VideoStreaming
{
    public class MockVideoStream : IVideoService
    {
        public bool StreamRunning => false;

        public StreamerConfig StreamerConfiguration { get; private set; } = new StreamerConfig();

        public Task RestartAsync(StreamerConfig config)
        {
            StreamerConfiguration = config;
            return Task.CompletedTask;
        }
    }
}
