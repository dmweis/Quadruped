using System;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Quadruped.WebInterface.RobotController
{
    public class Robot : IRobot
    {
        public event EventHandler<QuadrupedTelemetrics> NewTelemetricsUpdate;

        public Vector2 Direction { get; set; }
        public float Rotation { get; set; }

        private RobotConfig _gaitConfiguration = new RobotConfig();

        public RobotConfig GaitConfiguration
        {
            get => _gaitConfiguration;
            set
            {
                _gaitConfiguration = value;
                _interpolationController.Speed = GaitConfiguration.Speed;
            }
        }


        private readonly InterpolationController _interpolationController;
        private readonly ILogger<Robot> _logger;
        private Task _robotRunnerTask;
        private LegFlags _lastUsedCombo = LegFlags.RfLrCross;
        private bool _keepRunning = true;

        public Robot(InterpolationController interpolationController, IApplicationLifetime applicationLifetime, ILogger<Robot> logger)
        {
            _interpolationController = interpolationController;
            _interpolationController.NewTelemetricsUpdate += (sender, telemetrics) => NewTelemetricsUpdate?.Invoke(sender, telemetrics);
            _interpolationController.GaitEngineError += OnGaitEngineError;
            _logger = logger;
            _robotRunnerTask = Task.Run(RobotRunnerLoop);
            applicationLifetime.ApplicationStopped.Register(OnExit);
        }

        private void OnGaitEngineError(object o, Exception exception)
        {
            _logger.LogCritical("Gait engine expirienced critical error");
        }

        public async Task DisableMotors()
        {
            if (!_keepRunning || _robotRunnerTask.IsCompleted)
            {
                _logger.LogWarning("Can't disable robot when it's alredy disabled");
            }
            _keepRunning = false;
            await _robotRunnerTask;
            await Task.Run(() =>
            {
                _interpolationController.Stop();
            });
        }

        public void StartRobot()
        {
            if (_keepRunning || !_robotRunnerTask.IsCompleted)
            {
                _logger.LogWarning("Can't start robot when it's alredy running");
            }
            _interpolationController.Start();
            _interpolationController.EnqueueInitialStandup();
            _keepRunning = true;
            _robotRunnerTask = Task.Run(RobotRunnerLoop);
        }

        public Vector3 RelaxedStance { get; private set; } = Vector3.Zero;
        public Rotation RelaxedStanceRotation { get; private set; } = new Rotation();

        public void UpdateAboluteRelaxedStance(Vector3 transform, Rotation rotation)
        {
            RelaxedStance = transform;
            RelaxedStanceRotation = rotation;
            _interpolationController.RelaxedStance = _interpolationController
                .OriginalRelaxedStance
                .Transform(transform)
                .Rotate(rotation);
            _interpolationController.EnqueueMoveToRelaxed();
        }

        private async Task RobotRunnerLoop()
        {
            while (_keepRunning)
            {
                if (Direction != Vector2.Zero)
                {
                    if (GaitConfiguration.StepConfig == StepConfiguration.OneStep)
                    {
                        _interpolationController.EnqueueOneStep(Direction, GetNextLegCombo(), GaitConfiguration.FrontLegShift, GaitConfiguration.RearLegShift, GaitConfiguration.LiftHeight);
                    }
                    else
                    {
                        _interpolationController.EnqueueTwoSteps(Direction, GetNextLegCombo(), GaitConfiguration.FrontLegShift, GaitConfiguration.RearLegShift, GaitConfiguration.LiftHeight);
                    }
                    _interpolationController.WaitUntilMoveFinished();
                }
                else if (Rotation != 0)
                {
                    _interpolationController.EnqueueOneRotation(Rotation, GetNextLegCombo(), GaitConfiguration.LiftHeight);
                    _interpolationController.WaitUntilMoveFinished();
                }
                else
                {
                    await Task.Delay(10);
                }
            }
        }

        private LegFlags GetNextLegCombo()
        {
            _lastUsedCombo = _lastUsedCombo == LegFlags.RfLrCross ? LegFlags.LfRrCross : LegFlags.RfLrCross;
            return _lastUsedCombo;
        }

        private void OnExit()
        {
            _keepRunning = false;
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
