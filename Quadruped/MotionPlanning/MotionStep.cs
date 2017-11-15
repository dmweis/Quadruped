using System.Collections.Generic;

namespace Quadruped.MotionPlanning
{
    public class MotionStep
    {
        public StartingStanceOptions StargingStance { get; set; }
        public List<MotionPlanAction> Motions { get; set; }
    }
}
