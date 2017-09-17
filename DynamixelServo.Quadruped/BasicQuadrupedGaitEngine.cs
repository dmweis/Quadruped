using System;
using System.Numerics;

namespace DynamixelServo.Quadruped
{
    public class BasicQuadrupedGaitEngine : QuadrupedGaitEngine
    {
        private const int Speed = 10;

        private const int MaxForward = 5;
        private const int MinForward = -5;

        private const int LegDistance = 15;
        private const int LegHeight = -13;

        private bool _movingForward;
        private Vector3 _lastWrittenPosition = Vector3.Zero;

        public BasicQuadrupedGaitEngine(QuadrupedIkDriver driver) : base(driver)
        {
            Driver.Setup();
            Driver.StandUpfromGround();
            StartEngine();
        }

        protected override void EngineSpin()
        {
            var translate = Vector3.Zero;
            if (_movingForward)
            {
                translate.Y += Speed * 0.001f * TimeSincelastTick;
            }
            else
            {
                translate.Y -= Speed * 0.001f * TimeSincelastTick;
            }
            _lastWrittenPosition += translate;
            if (_movingForward && _lastWrittenPosition.Y > MaxForward)
            {
                _movingForward = false;
            }
            else if (!_movingForward && _lastWrittenPosition.Y < MinForward)
            {
                _movingForward = true;
            }
            Driver.MoveAbsoluteCenterMass(_lastWrittenPosition, LegDistance, LegHeight);
        }
    }
}
