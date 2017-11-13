using System.Numerics;
using System.Threading.Tasks;

namespace Quadruped.WebInterface.RobotController
{
    public class MockRobot : IRobot
    {
        public Vector2 Direction { get; set; }

        public float Rotation { get; set; }

        public RobotConfig GaitConfiguration { get; set; } = new RobotConfig();

        public void StartRobot()
        {
            
        }

        public Vector3 RelaxedStance { get; private set; }

        public Rotation RelaxedStanceRotation { get; private set; } = new Rotation();

        public void UpdateAboluteRelaxedStance(Vector3 transform, Rotation rotation)
        {
            RelaxedStance = transform;
            RelaxedStanceRotation = rotation;
        }

        public Task DisableMotors()
        {
            return Task.CompletedTask;
        }
    }
}
