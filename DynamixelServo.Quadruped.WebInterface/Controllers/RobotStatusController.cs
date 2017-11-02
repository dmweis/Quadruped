using System;
using DynamixelServo.Quadruped.WebInterface.RobotController;
using Microsoft.AspNetCore.Mvc;

namespace DynamixelServo.Quadruped.WebInterface.Controllers
{
    [Produces("application/json")]
    [Route("api/RobotStatus")]
    public class RobotStatusController : Controller
    {
        private readonly IRobot _robot;

        public RobotStatusController(IRobot robot)
        {
            _robot = robot;
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
