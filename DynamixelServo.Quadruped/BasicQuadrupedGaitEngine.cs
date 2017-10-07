using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace DynamixelServo.Quadruped
{
    public class BasicQuadrupedGaitEngine : QuadrupedGaitEngine
    {
        private const int Speed = 35;
        private float NextStepLength => Speed * 0.001f * TimeSincelastTick;

        private readonly Queue<LegPositions> _moves = new Queue<LegPositions>();

        private LegPositions _nextMove;

        private const float LegHeight = -9f;
        private const int LegDistanceLongitudinal = 17;
        private const int LegDistanceLateral = 10;

        private LegPositions RelaxedStance
        {
            get
            {
                return new LegPositions
                {
                    LeftFront = new Vector3(-LegDistanceLateral, LegDistanceLongitudinal, LegHeight),
                    RightFront = new Vector3(LegDistanceLateral, LegDistanceLongitudinal, LegHeight),
                    LeftRear = new Vector3(-LegDistanceLateral, -LegDistanceLongitudinal, LegHeight),
                    RightRear = new Vector3(LegDistanceLateral, -LegDistanceLongitudinal, LegHeight)
                }.Copy();
            }
        }

        private readonly LegPositions _lastWrittenPosition;

        public BasicQuadrupedGaitEngine(QuadrupedIkDriver driver) : base(driver)
        {
            Driver.Setup();
            _lastWrittenPosition = Driver.ReadCurrentLegPositions();
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
            try
            {
                Driver.MoveLegs(_lastWrittenPosition);
            }
            catch (IOException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                Console.ResetColor();
                throw;
            }
        }

        public void EnqueuePositions()
        {
            float average = (_lastWrittenPosition.LeftFront.Z +
                             _lastWrittenPosition.RightFront.Z +
                             _lastWrittenPosition.LeftRear.Z +
                             _lastWrittenPosition.RightRear.Z) / 4;
            if (average > LegHeight + 2)
            {
                _moves.Enqueue(new LegPositions
                {
                    LeftFront = new Vector3(-LegDistanceLateral, LegDistanceLongitudinal, 0),
                    RightFront = new Vector3(LegDistanceLateral, LegDistanceLongitudinal, 0),
                    LeftRear = new Vector3(-LegDistanceLateral, -LegDistanceLongitudinal, 0),
                    RightRear = new Vector3(LegDistanceLateral, -LegDistanceLongitudinal, 0)
                });
            }

            _moves.Enqueue(RelaxedStance);
            EnqueueOneStep();
        }

        public void EnqueueOneStep()
        {


            var nextStep = RelaxedStance;
            nextStep.Transform(new Vector3(0, 2, 1), LegFlags.LeftRear | LegFlags.RightFront);
            nextStep.Transform(new Vector3(0, -2, 0), LegFlags.LeftFront | LegFlags.RightRear);
            _moves.Enqueue(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(0, 2, -1), LegFlags.LeftRear | LegFlags.RightFront);
            nextStep.Transform(new Vector3(0, -2, 0), LegFlags.LeftFront | LegFlags.RightRear);
            _moves.Enqueue(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(0, 2, 1), LegFlags.LeftFront | LegFlags.RightRear);
            nextStep.Transform(new Vector3(0, -2, 0), LegFlags.LeftRear | LegFlags.RightFront);
            _moves.Enqueue(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(0, 2, -1), LegFlags.LeftFront | LegFlags.RightRear);
            nextStep.Transform(new Vector3(0, -2, 0), LegFlags.LeftRear | LegFlags.RightFront);
            _moves.Enqueue(nextStep);
        }
    }
}
