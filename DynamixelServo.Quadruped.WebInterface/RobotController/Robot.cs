using System;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace DynamixelServo.Quadruped.WebInterface.RobotController
{
    public class Robot : IRobot
    {
        public Vector2 Direction { get; set; }
        public float Rotation { get; set; }

        private readonly BasicQuadrupedGaitEngine _basicQuadrupedGaitEngine;
        private Task _robotRunnerTask;
        private LegFlags _lastUsedCombo = LegFlags.RfLrCross;
        private bool _keepRunning = true;

        public Robot(BasicQuadrupedGaitEngine basicQuadrupedGaitEngine, IApplicationLifetime applicationLifetime)
        {
            _basicQuadrupedGaitEngine = basicQuadrupedGaitEngine;
            _robotRunnerTask = Task.Run((Action)RobotRunnerLoop);
            applicationLifetime.ApplicationStopped.Register(() => _keepRunning = false);
        }

        public async Task DisableMotors()
        {
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
                throw new InvalidOperationException("Can't start robot when it's alredy running");
            }
            _keepRunning = true;
            _robotRunnerTask = Task.Run((Action)RobotRunnerLoop);
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
    }
}
