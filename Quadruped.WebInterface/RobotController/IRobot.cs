using System.Numerics;
using System.Threading.Tasks;

namespace Quadruped.WebInterface.RobotController
{
    public interface IRobot
    {
        Vector2 Direction { get; set; }
        float Rotation { get; set; }
        void StartRobot();
        Vector3 RelaxedStance { get; }
        void UpdateAboluteRelaxedStance(Vector3 transform);
        Task DisableMotors();
    }
}
