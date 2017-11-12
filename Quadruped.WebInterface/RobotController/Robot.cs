using System;
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

        private readonly BasicQuadrupedGaitEngine _basicQuadrupedGaitEngine;
        private readonly ILogger<Robot> _logger;
        private Task _robotRunnerTask;
        private LegFlags _lastUsedCombo = LegFlags.RfLrCross;
        private bool _keepRunning = true;

        public Robot(BasicQuadrupedGaitEngine basicQuadrupedGaitEngine, IApplicationLifetime applicationLifetime, ILogger<Robot> logger)
        {
            _basicQuadrupedGaitEngine = basicQuadrupedGaitEngine;
            _logger = logger;
            _robotRunnerTask = Task.Run((Action)RobotRunnerLoop);
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
                _basicQuadrupedGaitEngine.Stop();
                _basicQuadrupedGaitEngine.DisableTorqueOnMotors();
            });
        }

        public void StartRobot()
        {
            if (_keepRunning || !_robotRunnerTask.IsCompleted)
            {
                _logger.LogWarning("Can't start robot when it's alredy running");
            }
            _basicQuadrupedGaitEngine.StartEngine();
            _basicQuadrupedGaitEngine.EnqueueInitialStandup();
            _keepRunning = true;
            _robotRunnerTask = Task.Run((Action)RobotRunnerLoop);
        }

        public Vector3 RelaxedStance { get; private set; } = Vector3.Zero;

        public void UpdateAboluteRelaxedStance(Vector3 transform)
        {
            RelaxedStance = transform;
            var newRelaxed = _basicQuadrupedGaitEngine.RelaxedStance;
            newRelaxed.Transform(transform);
            _basicQuadrupedGaitEngine.RelaxedStance = newRelaxed;
        }

        private void RobotRunnerLoop()
        {
            while (_keepRunning)
            {
                if (Direction != Vector2.Zero)
                {
                    _basicQuadrupedGaitEngine.EnqueueOneStep(Direction, GetNextLegCombo());
                    _basicQuadrupedGaitEngine.WaitUntilCommandQueueIsEmpty();
                }
                else if (Rotation != 0)
                {
                    _basicQuadrupedGaitEngine.EnqueueOneRotation(Rotation, GetNextLegCombo());
                    _basicQuadrupedGaitEngine.WaitUntilCommandQueueIsEmpty();
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
