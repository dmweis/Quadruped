using System.Threading.Tasks;
using DynamixelServo.Quadruped.WebInterface.VideoStreaming;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DynamixelServo.Quadruped.WebInterface.Pages
{
    public class SettingsPageModel : PageModel
    {
        private readonly IVideoService _streamService;

        public SettingsPageModel(IVideoService streamService)
        {
            _streamService = streamService;
        }

        public void OnGet()
        {
            StreamConfiguration = _streamService.StreamerConfiguration;
        }

        [BindProperty]
        public StreamerConfig StreamConfiguration { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            await _streamService.RestartAsync(StreamConfiguration);

            return RedirectToPage("/VideoRobot");
        }
    }
}
