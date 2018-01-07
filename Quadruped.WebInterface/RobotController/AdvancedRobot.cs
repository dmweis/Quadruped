using System;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Quadruped.WebInterface.RobotController
{
    public class AdvancedRobot : IRobot
    {
        public event EventHandler<QuadrupedTelemetrics> NewTelemetricsUpdate;

        public Vector2 Direction
        {
            get => _interpolationController.Direction;
            set => _interpolationController.Direction = value;
        }

        public float Rotation { get; set; }
        public RobotConfig GaitConfiguration { get; set; } = new RobotConfig();

        public void StartRobot()
        {
            throw new NotImplementedException();
        }

        public Vector3 RelaxedStance { get; }
        public Rotation RelaxedStanceRotation { get; }

        private readonly AdvanceInterpolationController _interpolationController;
        private readonly ILogger<AdvancedRobot> _logger;


        public AdvancedRobot(AdvanceInterpolationController interpolationController, IApplicationLifetime applicationLifetime, ILogger<AdvancedRobot> logger)
        {
            _interpolationController = interpolationController;
            _interpolationController.NewTelemetricsUpdate += (sender, telemetrics) => NewTelemetricsUpdate?.Invoke(sender, telemetrics);
            _interpolationController.GaitEngineError += OnGaitEngineError;
            _logger = logger;
            applicationLifetime.ApplicationStopped.Register(OnExit);
        }

        private void OnGaitEngineError(object o, Exception exception)
        {
            _logger.LogCritical("Gait engine expirienced critical error");
        }

        public void UpdateAboluteRelaxedStance(Vector3 transform, Rotation rotation)
        {
            throw new NotImplementedException();
        }

        public async Task DisableMotors()
        {
            await Task.Run(() =>
            {
                _interpolationController.Stop();
            });
        }

        private void OnExit()
        {
            try
            {
                DisableMotors().Wait();
            }
            catch
            {
                _logger.LogError("Failed exiting Robot");
            }
        }
    }
}
