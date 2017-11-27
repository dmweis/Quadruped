namespace Quadruped.MotionPlanning
{
    public class MotionPlanAction
    {
        public Vector3Wrapper Motion { get; set; }
        public Rotation Rotation { get; set; }
        public LegFlags Legs { get; set; }
    }
}
