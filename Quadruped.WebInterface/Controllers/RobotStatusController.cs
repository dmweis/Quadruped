using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Quadruped.WebInterface.RobotController;
using Quadruped.WebInterface.VideoStreaming;

namespace Quadruped.WebInterface.Controllers
{
    [Produces("application/json")]
    [Route("api/RobotStatus")]
    public class RobotStatusController : Controller
    {
        private readonly IRobot _robot;
        private readonly IVideoService _videoService;
        private readonly ILogger<RobotStatusController> _logger;

        public RobotStatusController(IRobot robot, ILogger<RobotStatusController> logger, IVideoService videoService)
        {
            _robot = robot;
            _logger = logger;
            _videoService = videoService;
        }

        // GET: api/RobotStatus
        [HttpGet]
        public object Get()
        {
            return new
            {
                VideoStreamerOn = _videoService.StreamRunning,
                Options = Enum
                    .GetValues(typeof(RobotOperation))
                    .Cast<RobotOperation>()
                    .Select(flag => flag.ToString())
            };
        }

        // POST: api/RobotStatus
        [HttpPost]
        public void Post([FromBody]RobotCommand value)
        {
            if (value == null)
            {
                Response.StatusCode = 400;
                _logger.LogError("can't resolve argument");
                return;
            }
            switch (value.Operation)
            {
                case RobotOperation.StopMotors:
                    _robot.DisableMotors();
                    break;
                case RobotOperation.StartMotors:
                    _robot.StartRobot();
                    break;
                case RobotOperation.StopVideo:
                    _videoService.StopStream();
                    break;
                case RobotOperation.StartVideo:
                    _videoService.EnsureStreamOn();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class RobotCommand
    {
        public RobotOperation Operation { get; set; }
    }

    public enum RobotOperation
    {
        StopMotors,
        StartMotors,
        StopVideo,
        StartVideo
    }
}
