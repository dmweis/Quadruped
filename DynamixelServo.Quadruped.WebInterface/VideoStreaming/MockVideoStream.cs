using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace DynamixelServo.Quadruped.WebInterface.VideoStreaming
{
    public class MockVideoStream : IVideoService
    {
        public MockVideoStream(IOptions<StreamerConfig> config)
        {
            StreamerConfiguration = config.Value;
        }

        public bool StreamRunning => false;

        public StreamerConfig StreamerConfiguration { get; private set; }

        public Task RestartAsync(StreamerConfig config)
        {
            StreamerConfiguration = config;
            return Task.CompletedTask;
        }
    }
}
