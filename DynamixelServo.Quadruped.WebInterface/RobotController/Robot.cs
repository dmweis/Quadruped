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
        private readonly Task _robotRunnerTask;
        private LegFlags _lastUsedCombo = LegFlags.RfLrCross;
        private bool _keepRunning = true;

        public Robot(BasicQuadrupedGaitEngine basicQuadrupedGaitEngine, IApplicationLifetime applicationLifetime)
        {
            _basicQuadrupedGaitEngine = basicQuadrupedGaitEngine;
            _robotRunnerTask = Task.Run((Action)RobotRunnerLoop);
            applicationLifetime.ApplicationStopped.Register(() => _keepRunning = false);
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
