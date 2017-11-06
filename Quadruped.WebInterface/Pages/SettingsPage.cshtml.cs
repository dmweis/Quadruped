using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Quadruped.WebInterface.VideoStreaming;

namespace Quadruped.WebInterface.Pages
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
            _streamService.StreamerConfiguration = StreamConfiguration;
            await _streamService.RestartAsync();
            return RedirectToPage("/VideoRobot");
        }
    }
}
