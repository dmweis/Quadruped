using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Quadruped.WebInterface.VideoStreaming
{
    public class MockVideoStream : IVideoService
    {
        public MockVideoStream(IOptions<StreamerConfig> config)
        {
            StreamerConfiguration = config.Value;
        }

        public bool StreamRunning { get; private set; }

        public StreamerConfig StreamerConfiguration { get; set; }

        public Task RestartAsync()
        {
            StreamRunning = true;
            return Task.CompletedTask;
        }

        public Task EnsureStreamOn()
        {
            return RestartAsync();
        }
    }
}
