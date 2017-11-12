using System.Numerics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Quadruped.WebInterface.RobotController;
using Quadruped.WebInterface.VideoStreaming;

namespace Quadruped.WebInterface.Pages
{
    public class SettingsPageModel : PageModel
    {
        private readonly IVideoService _streamService;
        private readonly IRobot _robotController;

        public SettingsPageModel(IVideoService streamService, IRobot robotController)
        {
            _streamService = streamService;
            _robotController = robotController;
        }

        public void OnGet()
        {
            StreamConfiguration = _streamService.StreamerConfiguration;
            var relaxed = _robotController.RelaxedStance;
            RelaxedStance = new Vector3Class{X = relaxed.X, Y = relaxed.Y, Z = relaxed.Z};
            RobotConfiguration = _robotController.GaitConfiguration;
        }

        [BindProperty]
        public StreamerConfig StreamConfiguration { get; set; }
        [BindProperty]
        public Vector3Class RelaxedStance { get; set; }
        [BindProperty]
        public RobotConfig RobotConfiguration { get; set; }

        public RobotConfig DefaultRobotConfiguration => new RobotConfig();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            if (_streamService.StreamerConfiguration != StreamConfiguration)
            {
                _streamService.StreamerConfiguration = StreamConfiguration;
                await _streamService.RestartAsync();
            }
            _robotController.UpdateAboluteRelaxedStance((Vector3)RelaxedStance);
            _robotController.GaitConfiguration = RobotConfiguration;
            return RedirectToPage("/VideoRobot");
        }
    }

    public class Vector3Class
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public static explicit operator Vector3(Vector3Class original)
        {
            return new Vector3(original.X, original.Y, original.Z);
        }
    }
}
