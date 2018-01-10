using System;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Quadruped.WebInterface.RobotController
{
    public class SmartRobot : IRobot
    {
        private readonly SmartQuadrupedController _controller;
        private readonly ILogger<Robot> _logger;

        public event EventHandler<QuadrupedTelemetrics> NewTelemetricsUpdate;

        public Vector2 Direction
        {
            get => _controller.Direction;
            set => _controller.Direction = value;
        }

        public float Rotation
        {
            get => _controller.Rotation;
            set => _controller.Rotation = value;
        }

        public Vector3 RelaxedStance { get; private set; } = Vector3.Zero;
        public Rotation RelaxedStanceRotation { get; private set; } = new Rotation();

        public RobotConfig GaitConfiguration { get; set; }


        public void StartRobot()
        {
            throw new NotImplementedException();
        }

        public SmartRobot(SmartQuadrupedController controller, IApplicationLifetime applicationLifetime)
        {
            _controller = controller;
            _controller.NewTelemetricsUpdate += (sender, telemetrics) => NewTelemetricsUpdate?.Invoke(sender, telemetrics);
            applicationLifetime.ApplicationStopping.Register(() => _controller.Stop());
        }

        public void UpdateAboluteRelaxedStance(Vector3 transform, Rotation rotation)
        {
            RelaxedStance = transform;
            RelaxedStanceRotation = rotation;
            _controller.RelaxedStance = SmartQuadrupedController
                .OriginalRelaxedStance
                .Transform(transform)
                .Rotate(rotation);
        }

        public Task DisableMotors()
        {
            throw new NotImplementedException();
        }
    }
}
