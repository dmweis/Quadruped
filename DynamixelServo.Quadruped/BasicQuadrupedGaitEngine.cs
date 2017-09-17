using System;
using System.Numerics;

namespace DynamixelServo.Quadruped
{
    public class BasicQuadrupedGaitEngine : QuadrupedGaitEngine
    {
        private const int Speed = 12;
        private float NextStepLength => Speed * 0.001f * TimeSincelastTick;

        private int _currentIndex;

        private readonly Vector3[] _positions =
        {
            new Vector3(-5, 5, 3),
            new Vector3(5, 5, -3),
            new Vector3(5, -5, 3),
            new Vector3(-5, -5, -3)
        };

        private const float LegHeight = -11f;
        private const int LegDistance = 15;

        private Vector3 _lastWrittenPosition = Vector3.Zero;

        public BasicQuadrupedGaitEngine(QuadrupedIkDriver driver) : base(driver)
        {
            Driver.Setup();
            Driver.StandUpfromGround();
            StartEngine();
        }

        protected override void EngineSpin()
        {
            if (_lastWrittenPosition.Similar(_positions[_currentIndex], 0.25f))
            {
                _currentIndex++;
                if (_currentIndex >= _positions.Length)
                {
                    _currentIndex = 0;
                }
            }
            _lastWrittenPosition =
                _lastWrittenPosition.MoveTowards(_positions[_currentIndex], NextStepLength);
            Driver.MoveAbsoluteCenterMass(_lastWrittenPosition, LegDistance, LegHeight);
        }
    }
}
