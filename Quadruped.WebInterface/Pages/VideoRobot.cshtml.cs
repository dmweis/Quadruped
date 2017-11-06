using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Quadruped.WebInterface.VideoStreaming;

namespace Quadruped.WebInterface.Pages
{
    public class VideoRobotModel : PageModel
    {
        private readonly IVideoService _videoService;

        public VideoRobotModel(IVideoService videoService)
        {
            _videoService = videoService;
        }

        public async Task OnGetAsync()
        {
            await _videoService.EnsureStreamOn();
        }
    }
}