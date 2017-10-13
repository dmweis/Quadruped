using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace DynamixelServo.Quadruped
{
    public class BasicQuadrupedGaitEngine : QuadrupedGaitEngine
    {
        private const int Speed = 30;
        private float NextStepLength => Speed * 0.001f * TimeSincelastTick;

        private readonly Queue<LegPositions> _moves = new Queue<LegPositions>();

        private LegPositions _nextMove;

        private const float LegHeight = -9f;
        //private const int LegDistanceLongitudinal = 17;
        //private const int LegDistanceLateral = 10;
        private const int LegDistanceLongitudinal = 15;
        private const int LegDistanceLateral = 15;

        private LegPositions RelaxedStance => new LegPositions
        {
            LeftFront = new Vector3(-LegDistanceLateral, LegDistanceLongitudinal, LegHeight),
            RightFront = new Vector3(LegDistanceLateral, LegDistanceLongitudinal, LegHeight),
            LeftRear = new Vector3(-LegDistanceLateral, -LegDistanceLongitudinal, LegHeight),
            RightRear = new Vector3(LegDistanceLateral, -LegDistanceLongitudinal, LegHeight)
        };

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
                Driver.MoveLegsSynced(_lastWrittenPosition);
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
        }

        public void EnqueueOneStep(Vector2 direction, LegFlags forwardMovingLegs = LegFlags.RfLrCross, float frontLegShift = 2, float rearLegShift = 1, float liftHeight = 2, bool normalize = true)
        {
            if (direction.X == 0f && direction.Y == 0f)
            {
                return;
            }
            if (normalize)
            {
                direction = direction.Normal();
            }

            if (forwardMovingLegs != LegFlags.LfRrCross && forwardMovingLegs != LegFlags.RfLrCross)
            {
                throw new ArgumentException(nameof(forwardMovingLegs));
            }
            LegFlags backwardsMovingLegs = forwardMovingLegs == LegFlags.LfRrCross ? LegFlags.RfLrCross : LegFlags.LfRrCross;
            var nextStep = RelaxedStance;

            // Move LR and RF forward
            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(frontLegShift * direction.X, frontLegShift * direction.Y, liftHeight), forwardMovingLegs);
            nextStep.Transform(new Vector3(-rearLegShift * direction.X, -rearLegShift * direction.Y, 0), backwardsMovingLegs);
            _moves.Enqueue(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(frontLegShift * direction.X, frontLegShift * direction.Y, -liftHeight), forwardMovingLegs);
            nextStep.Transform(new Vector3(-rearLegShift * direction.X, -rearLegShift * direction.Y, 0), backwardsMovingLegs);
            _moves.Enqueue(nextStep);

            // Move all
            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(-frontLegShift * direction.X, -frontLegShift * direction.Y, 0));
            _moves.Enqueue(nextStep);

            // Move RR and LF forward
            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(frontLegShift * direction.X, frontLegShift * direction.Y, liftHeight), backwardsMovingLegs);
            nextStep.Transform(new Vector3(-rearLegShift * direction.X, -rearLegShift * direction.Y, 0), forwardMovingLegs);
            _moves.Enqueue(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(frontLegShift * direction.X, frontLegShift * direction.Y, -liftHeight), backwardsMovingLegs);
            nextStep.Transform(new Vector3(-rearLegShift * direction.X, -rearLegShift * direction.Y, 0), forwardMovingLegs);
            _moves.Enqueue(nextStep);

        }
    }
}
