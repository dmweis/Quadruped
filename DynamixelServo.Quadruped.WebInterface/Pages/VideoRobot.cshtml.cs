using System.Threading.Tasks;
using DynamixelServo.Quadruped.WebInterface.VideoStreaming;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DynamixelServo.Quadruped.WebInterface.Pages
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