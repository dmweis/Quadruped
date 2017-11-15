namespace Quadruped.MotionPlanning
{
    class RotationAction : MotionPlanAction
    {
        public Rotation Rotation { get; set; }
        public LegFlags Legs { get; set; }

    }
}
