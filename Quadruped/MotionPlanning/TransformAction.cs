using System.Numerics;

namespace Quadruped.MotionPlanning
{
    public class TransformAction : MotionPlanAction
    {
        public Vector3Wrapper Motion { get; set; }
        public LegFlags Legs { get; set; }
    }
}
