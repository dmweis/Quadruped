using System.Numerics;
using System.Threading.Tasks;

namespace Quadruped.WebInterface.RobotController
{
    public class MockRobot : IRobot
    {
        public Vector2 Direction { get; set; }

        public float Rotation { get; set; }

        public void StartRobot()
        {
            
        }

        public Task DisableMotors()
        {
            return Task.CompletedTask;
        }
    }
}
