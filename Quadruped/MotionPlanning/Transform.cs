using System.Numerics;

namespace Quadruped.MotionPlanning
{
    public class Transform
    {
        public Vector3 Motion { get; set; }
        public LegFlags Legs { get; set; }
    }
}
