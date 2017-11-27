using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Quadruped.MotionPlanning;

namespace Quadruped.WebInterface.Controllers
{
    [Produces("application/json")]
    [Route("api/MotionPlanning")]
    public class MotionPlanningController : Controller
    {
        // GET: api/MotionPlanning
        [HttpGet]
        public MotionPlan Get()
        {
            return new MotionPlan
            {
                Movements = new List<MotionStep>
                {
                    new MotionStep
                    {
                        StargingStance = StartingStanceOptions.Relaxed,
                        Motions = new List<MotionPlanAction>
                        {
                            new MotionPlanAction{Legs = LegFlags.RightFront, Motion = new Vector3Wrapper{Z = 5}},
                            new MotionPlanAction{Legs = LegFlags.Left & LegFlags.RightRear, Motion = new Vector3Wrapper{X = 2, Y = -2}},
                            new MotionPlanAction{Legs = LegFlags.Left & LegFlags.RightRear, Rotation = new Rotation(0, 0, 0)}
                        }
                    },
                    new MotionStep
                    {
                        StargingStance = StartingStanceOptions.Previous,
                        Motions = new List<MotionPlanAction>
                        {
                            new MotionPlanAction{Legs = LegFlags.RightFront, Motion = new Vector3Wrapper{Z = -5}},
                            new MotionPlanAction{Legs = LegFlags.Left & LegFlags.RightRear, Motion = new Vector3Wrapper{X = -2, Y = 2}}
                        }
                    }
                }
            };
        }
        
        // POST: api/MotionPlanning
        [HttpPost]
        public void Post([FromBody]MotionPlan value)
        {
            Response.StatusCode = 501;
        }        
    }
}
