using System.Numerics;

namespace DynamixelServo.Quadruped.WebInterface.RobotController
{
    public class MockRobot : IRobot
    {
        public Vector2 Direction { get; set; }

        public float Rotation { get; set; }
    }
}
