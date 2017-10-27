using System.Numerics;

namespace DynamixelServo.Quadruped.WebInterface.RobotController
{
    public interface IRobot
    {
        Vector2 Direction { get; set; }
        float Rotation { get; set; }
    }
}
