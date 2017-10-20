using System;
using System.Numerics;
using System.Threading.Tasks;
using DynamixelServo.Quadruped;

namespace dynamixelServo.Quadruped.WebInterface
{
    public class RobotController
    {
        private readonly BasicQuadrupedGaitEngine _basicQuadrupedGaitEngine;
        private readonly Task _robotRunnerTask;
        private LegFlags _lastUsedCombo = LegFlags.RfLrCross;
        private Vector2 _direction;
        private Vector2 _rotation;

        public RobotController(BasicQuadrupedGaitEngine basicQuadrupedGaitEngine)
        {
            _basicQuadrupedGaitEngine = basicQuadrupedGaitEngine;
            _robotRunnerTask = Task.Run((Action)RobotRunnerLoop);
        }

        private void RobotRunnerLoop()
        {
            while (true)
            {
                if (_direction != Vector2.Zero)
                {
                    _basicQuadrupedGaitEngine.EnqueueOneStep(_direction, GetNextLegCombo());
                    _basicQuadrupedGaitEngine.WaitUntilCommandQueueIsEmpty();
                }
                else if (_rotation != Vector2.Zero)
                {
                    _basicQuadrupedGaitEngine.EnqueueOneRotation(_rotation.X * 6, GetNextLegCombo());
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

        public void Rotate(Vector2 direction)
        {
            _rotation = direction;
        }
    }
}
