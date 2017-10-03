using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DynamixelServo.Quadruped
{
    public class BasicQuadrupedGaitEngine : QuadrupedGaitEngine
    {
        private const int Speed = 22;
        private float NextStepLength => Speed * 0.001f * TimeSincelastTick;

        private readonly Queue<MotorPositions> _moves = new Queue<MotorPositions>();

        private MotorPositions _nextMove;

        private const float LegHeight = -11f;
        private const int LegDistance = 15;

        private MotorPositions RelaxedStance
        {
            get
            {
                return new MotorPositions
                {
                    LeftFront = new Vector3(-LegDistance, LegDistance, LegHeight),
                    RightFront = new Vector3(LegDistance, LegDistance, LegHeight),
                    LeftRear = new Vector3(-LegDistance, -LegDistance, LegHeight),
                    RightRear = new Vector3(LegDistance, -LegDistance, LegHeight)
                }.Copy();
            }
        }

        private readonly MotorPositions _lastWrittenPosition;

        public BasicQuadrupedGaitEngine(QuadrupedIkDriver driver) : base(driver)
        {
            Driver.Setup();
            _lastWrittenPosition = new MotorPositions(driver);
            EnqueuePositions();
            _nextMove = _moves.Dequeue();
            StartEngine();
        }

        protected override void EngineSpin()
        {
            if (_lastWrittenPosition.MoveFinished(_nextMove))
            {
                if (_moves.Count > 0)
                {
                    _nextMove = _moves.Dequeue();
                }
            }
            _lastWrittenPosition.MoveTowards(_nextMove, NextStepLength);
            _lastWrittenPosition.MoveRobot(Driver);
        }

        public void EnqueuePositions()
        {
            float average = (_lastWrittenPosition.LeftFront.Z +
                             _lastWrittenPosition.RightFront.Z +
                             _lastWrittenPosition.LeftRear.Z +
                             _lastWrittenPosition.RightRear.Z) / 4;
            if (average > -9)
            {
                _moves.Enqueue(new MotorPositions
                {
                    LeftFront = new Vector3(-LegDistance, LegDistance, 0),
                    RightFront = new Vector3(LegDistance, LegDistance, 0),
                    LeftRear = new Vector3(-LegDistance, -LegDistance, 0),
                    RightRear = new Vector3(LegDistance, -LegDistance, 0)
                });
            }
            
            _moves.Enqueue(RelaxedStance);
            EnqueueOneStep();
        }

        public void EnqueueOneStep()
        {
            var nextStep = RelaxedStance;
            nextStep.Transform(new Vector3(3, 0, 0));
            _moves.Enqueue(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(0, 0, 3), LegFlags.RightFront);
            _moves.Enqueue(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(0, 8, 0), LegFlags.RightFront);
            _moves.Enqueue(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(0, 0, -3), LegFlags.RightFront);
            _moves.Enqueue(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(-6, -3, 0));
            _moves.Enqueue(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(0, 0, 3), LegFlags.LeftFront);
            _moves.Enqueue(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(0, 8, 0), LegFlags.LeftFront);
            _moves.Enqueue(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(0, 0, -3), LegFlags.LeftFront);
            _moves.Enqueue(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(6, -3, 0));
            _moves.Enqueue(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(0, 0, 3), LegFlags.RightRear);
            _moves.Enqueue(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(0, 8, 0), LegFlags.RightRear);
            _moves.Enqueue(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(0, 0, -3), LegFlags.RightRear);
            _moves.Enqueue(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(-6, -3, 0));
            _moves.Enqueue(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(0, 0, 3), LegFlags.LeftRear);
            _moves.Enqueue(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(0, 8, 0), LegFlags.LeftRear);
            _moves.Enqueue(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(0, 0, -3), LegFlags.LeftRear);
            _moves.Enqueue(nextStep);
        }
    }
}
