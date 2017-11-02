using Microsoft.AspNetCore.Mvc;

namespace DynamixelServo.Quadruped.WebInterface.Controllers
{
    [Produces("application/json")]
    [Route("api/RobotStatus")]
    public class RobotStatusController : Controller
    {
        // GET: api/RobotStatus
        [HttpGet]
        public object Get()
        {
            return new {Status = "Ready"};
        }
        
        // POST: api/RobotStatus
        [HttpPost]
        public void Post([FromBody]RobotStatusData value)
        {
        }
        
    }

    public class RobotStatusData
    {
        public RobotOperation Operation { get; set; }
    }

    public enum RobotOperation
    {
        DisableMotors,
        Sit,
        Stand,
        StopVideo,
        StartVideo

    }
}
