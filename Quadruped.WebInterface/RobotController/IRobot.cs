using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Quadruped.WebInterface.RobotController
{
    public interface IRobot
    {
        event EventHandler<QuadrupedTelemetrics> NewTelemetricsUpdate;
        Vector2 Direction { get; set; }
        float Rotation { get; set; }
        RobotConfig GaitConfiguration { get; set; }
        void StartRobot();
        Vector3 RelaxedStance { get; }
        Rotation RelaxedStanceRotation { get; }
        void UpdateAboluteRelaxedStance(Vector3 transform, Rotation rotation);
        Task DisableMotors();
    }
}
