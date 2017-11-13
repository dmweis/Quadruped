using System.Numerics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Quadruped.WebInterface.RobotController
{
    public class Robot : IRobot
    {
        public Vector2 Direction { get; set; }
        public float Rotation { get; set; }

        private RobotConfig _gaitConfiguration = new RobotConfig();

        public RobotConfig GaitConfiguration
        {
            get => _gaitConfiguration;
            set
            {
                _gaitConfiguration = value;
                _interpolationGaitEngine.Speed = GaitConfiguration.Speed;
            }
        }


        private readonly InterpolationGaitEngine _interpolationGaitEngine;
        private readonly ILogger<Robot> _logger;
        private Task _robotRunnerTask;
        private LegFlags _lastUsedCombo = LegFlags.RfLrCross;
        private bool _keepRunning = true;

        public Robot(InterpolationGaitEngine interpolationGaitEngine, IApplicationLifetime applicationLifetime, ILogger<Robot> logger)
        {
            _interpolationGaitEngine = interpolationGaitEngine;
            _logger = logger;
            _robotRunnerTask = Task.Run(RobotRunnerLoop);
            applicationLifetime.ApplicationStopped.Register(OnExit);
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
                _interpolationGaitEngine.Stop();
                _interpolationGaitEngine.DisableTorqueOnMotors();
            });
        }

        public void StartRobot()
        {
            if (_keepRunning || !_robotRunnerTask.IsCompleted)
            {
                _logger.LogWarning("Can't start robot when it's alredy running");
            }
            _interpolationGaitEngine.StartEngine();
            _interpolationGaitEngine.EnqueueInitialStandup();
            _keepRunning = true;
            _robotRunnerTask = Task.Run(RobotRunnerLoop);
        }

        public Vector3 RelaxedStance { get; private set; } = Vector3.Zero;

        public void UpdateAboluteRelaxedStance(Vector3 transform)
        {
            RelaxedStance = transform;
            var newRelaxed = _interpolationGaitEngine.OriginalRelaxedStance;
            newRelaxed.Transform(transform);
            _interpolationGaitEngine.RelaxedStance = newRelaxed;
        }

        private async Task RobotRunnerLoop()
        {
            while (_keepRunning)
            {
                if (Direction != Vector2.Zero)
                {
                    if (GaitConfiguration.StepConfig == StepConfiguration.OneStep)
                    {
                        _interpolationGaitEngine.EnqueueOneStep(Direction, GetNextLegCombo(), GaitConfiguration.FrontLegShift, GaitConfiguration.RearLegShift, GaitConfiguration.LiftHeight);
                    }
                    else
                    {
                        _interpolationGaitEngine.EnqueueTwoSteps(Direction, GetNextLegCombo(), GaitConfiguration.FrontLegShift, GaitConfiguration.RearLegShift, GaitConfiguration.LiftHeight);
                    }
                    _interpolationGaitEngine.WaitUntilCommandQueueIsEmpty();
                }
                else if (Rotation != 0)
                {
                    _interpolationGaitEngine.EnqueueOneRotation(Rotation, GetNextLegCombo(), GaitConfiguration.LiftHeight);
                    _interpolationGaitEngine.WaitUntilCommandQueueIsEmpty();
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
