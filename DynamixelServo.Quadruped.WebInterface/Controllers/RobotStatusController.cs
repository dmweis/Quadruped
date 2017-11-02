using System;
using DynamixelServo.Quadruped.WebInterface.RobotController;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DynamixelServo.Quadruped.WebInterface.Controllers
{
    [Produces("application/json")]
    [Route("api/RobotStatus")]
    public class RobotStatusController : Controller
    {
        private readonly IRobot _robot;
        private readonly ILogger<RobotStatusController> _logger;

        public RobotStatusController(IRobot robot, ILogger<RobotStatusController> logger)
        {
            _robot = robot;
            _logger = logger;
        }

        // GET: api/RobotStatus
        [HttpGet]
        public object Get()
        {
            return new { Status = "Ready" };
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
                case RobotOperation.DisableMotors:
                    _robot.DisableMotors();
                    break;
                case RobotOperation.StartMotors:
                    _robot.StartRobot();
                    break;
                case RobotOperation.StopVideo:
                    break;
                case RobotOperation.StartVideo:
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
        DisableMotors,
        StartMotors,
        StopVideo,
        StartVideo
    }
}
