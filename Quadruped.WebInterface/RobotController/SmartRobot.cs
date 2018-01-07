using System;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace Quadruped.WebInterface.RobotController
{
    public class SmartRobot : IRobot
    {
        private readonly SmartQuadrupedController _controller;
        public event EventHandler<QuadrupedTelemetrics> NewTelemetricsUpdate;

        public Vector2 Direction
        {
            get => _controller.Direction;
            set => _controller.Direction = value;
        }

        public float Rotation { get; set; }
        public RobotConfig GaitConfiguration { get; set; }
        public void StartRobot()
        {
            throw new NotImplementedException();
        }

        public Vector3 RelaxedStance { get; }
        public Rotation RelaxedStanceRotation { get; }

        public SmartRobot(SmartQuadrupedController controller, IApplicationLifetime applicationLifetime)
        {
            _controller = controller;
            applicationLifetime.ApplicationStopping.Register(() => _controller.Stop());
        }

        public void UpdateAboluteRelaxedStance(Vector3 transform, Rotation rotation)
        {
            throw new NotImplementedException();
        }

        public Task DisableMotors()
        {
            throw new NotImplementedException();
        }
    }
}
