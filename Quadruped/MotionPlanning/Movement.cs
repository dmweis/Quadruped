using System.Collections.Generic;

namespace Quadruped.MotionPlanning
{
    public class Movement
    {
        public StartingStanceOptions StargingStance { get; set; }
        public List<Transform> Motions { get; set; }
    }
}
