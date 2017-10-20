using System;
using System.Numerics;
using System.Threading.Tasks;
using DynamixelServo.Quadruped;
using Microsoft.AspNetCore.Hosting;

namespace dynamixelServo.Quadruped.WebInterface
{
    public class RobotController
    {
        private readonly BasicQuadrupedGaitEngine _basicQuadrupedGaitEngine;
        private readonly Task _robotRunnerTask;
        private LegFlags _lastUsedCombo = LegFlags.RfLrCross;
        private Vector2 _direction;
        private float _rotation;
        private bool _keepRunning = true;

        public RobotController(BasicQuadrupedGaitEngine basicQuadrupedGaitEngine, IApplicationLifetime applicationLifetime)
        {
            _basicQuadrupedGaitEngine = basicQuadrupedGaitEngine;
            _robotRunnerTask = Task.Run((Action)RobotRunnerLoop);
            applicationLifetime.ApplicationStopped.Register(() => _keepRunning = false);
        }

        private void RobotRunnerLoop()
        {
            while (_keepRunning)
            {
                if (_direction != Vector2.Zero)
                {
                    _basicQuadrupedGaitEngine.EnqueueOneStep(_direction, GetNextLegCombo());
                    _basicQuadrupedGaitEngine.WaitUntilCommandQueueIsEmpty();
                }
                else if (_rotation != 0)
                {
                    _basicQuadrupedGaitEngine.EnqueueOneRotation(_rotation, GetNextLegCombo());
                    _basicQuadrupedGaitEngine.WaitUntilCommandQueueIsEmpty();
                }
            }
        }

        private LegFlags GetNextLegCombo()
        {
            _lastUsedCombo = _lastUsedCombo == LegFlags.RfLrCross ? LegFlags.LfRrCross : LegFlags.RfLrCross;
            return _lastUsedCombo;
        }

        public void Move(Vector2 direction)
        {
            _direction = direction;
        }

        public void Rotate(float rotation)
        {
            _rotation = rotation;
        }
    }
}
