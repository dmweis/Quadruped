using System;
using System.Numerics;

namespace Quadruped
{
    public class InterpolationController : IDisposable
    {
        private readonly InterpolationGaitEngine _engine;

        private const float LegHeight = -9f;
        //private const int LegDistanceLongitudinal = 17;
        //private const int LegDistanceLateral = 10;
        private const int LegDistanceLongitudinal = 15;
        private const int LegDistanceLateral = 15;


        public int Speed
        {
            get => _engine.Speed;
            set => _engine.Speed = value;
        }


        public LegPositions OriginalRelaxedStance => new LegPositions
        {
            LeftFront = new Vector3(-LegDistanceLateral, LegDistanceLongitudinal, LegHeight),
            RightFront = new Vector3(LegDistanceLateral, LegDistanceLongitudinal, LegHeight),
            LeftRear = new Vector3(-LegDistanceLateral, -LegDistanceLongitudinal, LegHeight),
            RightRear = new Vector3(LegDistanceLateral, -LegDistanceLongitudinal, LegHeight)
        };

        private LegPositions _relaxedStance;

        public LegPositions RelaxedStance
        {
            get => _relaxedStance.Copy();
            set => _relaxedStance = value;
        }

        public InterpolationController(InterpolationGaitEngine engine)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            EnqueueInitialStandup();
        }

        public void EnqueueInitialStandup()
        {
            var currentPositions = _engine.ReadCurrentLegPositions();
            _engine.AddStep(currentPositions);
            float average = (currentPositions.LeftFront.Z +
                             currentPositions.RightFront.Z +
                             currentPositions.LeftRear.Z +
                             currentPositions.RightRear.Z) / 4;
            if (average > LegHeight + 2)
            {
                _engine.AddStep(new LegPositions
                {
                    LeftFront = new Vector3(-LegDistanceLateral, LegDistanceLongitudinal, 0),
                    RightFront = new Vector3(LegDistanceLateral, LegDistanceLongitudinal, 0),
                    LeftRear = new Vector3(-LegDistanceLateral, -LegDistanceLongitudinal, 0),
                    RightRear = new Vector3(LegDistanceLateral, -LegDistanceLongitudinal, 0)
                });
            }

            _engine.AddStep(RelaxedStance);
        }

        public void WaitUntilMoveFinished() => _engine.WaitUntilCommandQueueIsEmpty();

        public void Start() => _engine.StartEngine();

        public void Stop()
        {
            _engine.Stop();
            _engine.DisableTorqueOnMotors();
        }

        public void EnqueueMoveToRelaxed()
        {
            _engine.AddStep(RelaxedStance);
        }

        public void EnqueueOneStep(Vector2 direction, LegFlags forwardMovingLegs = LegFlags.RfLrCross, float frontLegShift = 2, float rearLegShift = 1, float liftHeight = 2, bool normalize = true)
        {
            // identity comparasion to prevent error on float.NaN
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
                throw new ArgumentException($"{nameof(forwardMovingLegs)} has to be {nameof(LegFlags.RfLrCross)} or {nameof(LegFlags.LfRrCross)}");
            }
            LegFlags backwardsMovingLegs = forwardMovingLegs == LegFlags.LfRrCross ? LegFlags.RfLrCross : LegFlags.LfRrCross;
            var nextStep = RelaxedStance;

            // Move LR and RF forward
            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(frontLegShift * direction.X, frontLegShift * direction.Y, liftHeight), forwardMovingLegs);
            nextStep.Transform(new Vector3(-rearLegShift * direction.X, -rearLegShift * direction.Y, 0), backwardsMovingLegs);
            _engine.AddStep(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(frontLegShift * direction.X, frontLegShift * direction.Y, -liftHeight), forwardMovingLegs);
            nextStep.Transform(new Vector3(-rearLegShift * direction.X, -rearLegShift * direction.Y, 0), backwardsMovingLegs);
            _engine.AddStep(nextStep);

            // Move all
            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(-frontLegShift * direction.X, -frontLegShift * direction.Y, 0));
            _engine.AddStep(nextStep);

            // Move RR and LF forward
            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(frontLegShift * direction.X, frontLegShift * direction.Y, liftHeight), backwardsMovingLegs);
            nextStep.Transform(new Vector3(-rearLegShift * direction.X, -rearLegShift * direction.Y, 0), forwardMovingLegs);
            _engine.AddStep(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(frontLegShift * direction.X, frontLegShift * direction.Y, -liftHeight), backwardsMovingLegs);
            nextStep.Transform(new Vector3(-rearLegShift * direction.X, -rearLegShift * direction.Y, 0), forwardMovingLegs);
            _engine.AddStep(nextStep);
        }

        public void EnqueueTwoSteps(Vector2 direction, LegFlags forwardMovingLegs = LegFlags.RfLrCross, float frontLegShift = 2, float rearLegShift = 1, float liftHeight = 2, bool normalize = true)
        {
            // identity comparasion to prevent error on float.NaN
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
                throw new ArgumentException($"{nameof(forwardMovingLegs)} has to be {nameof(LegFlags.RfLrCross)} or {nameof(LegFlags.LfRrCross)}");
            }
            LegFlags backwardsMovingLegs = forwardMovingLegs == LegFlags.LfRrCross ? LegFlags.RfLrCross : LegFlags.LfRrCross;
            var nextStep = RelaxedStance;

            // Move LR and RF forward
            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(frontLegShift * direction.X, frontLegShift * direction.Y, liftHeight), forwardMovingLegs);
            nextStep.Transform(new Vector3(-rearLegShift * direction.X, -rearLegShift * direction.Y, 0), backwardsMovingLegs);
            _engine.AddStep(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(frontLegShift * direction.X, frontLegShift * direction.Y, -liftHeight), forwardMovingLegs);
            nextStep.Transform(new Vector3(-rearLegShift * direction.X, -rearLegShift * direction.Y, 0), backwardsMovingLegs);
            _engine.AddStep(nextStep);

            // Move all
            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(-frontLegShift * direction.X, -frontLegShift * direction.Y, 0));
            _engine.AddStep(nextStep);

            // Move RR and LF forward for two steps
            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(frontLegShift * 2 * direction.X, frontLegShift * 2 * direction.Y, liftHeight), backwardsMovingLegs);
            nextStep.Transform(new Vector3(-rearLegShift * 2 * direction.X, -rearLegShift * 2 * direction.Y, 0), forwardMovingLegs);
            _engine.AddStep(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(frontLegShift * 2 * direction.X, frontLegShift * 2 * direction.Y, -liftHeight), backwardsMovingLegs);
            nextStep.Transform(new Vector3(-rearLegShift * 2 * direction.X, -rearLegShift * 2 * direction.Y, 0), forwardMovingLegs);
            _engine.AddStep(nextStep);

            // Move all
            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(-frontLegShift * direction.X, -frontLegShift * direction.Y, 0));
            _engine.AddStep(nextStep);

            // Move LR and RF forward
            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(frontLegShift * direction.X, frontLegShift * direction.Y, liftHeight), forwardMovingLegs);
            nextStep.Transform(new Vector3(-rearLegShift * direction.X, -rearLegShift * direction.Y, 0), backwardsMovingLegs);
            _engine.AddStep(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(frontLegShift * direction.X, frontLegShift * direction.Y, -liftHeight), forwardMovingLegs);
            nextStep.Transform(new Vector3(-rearLegShift * direction.X, -rearLegShift * direction.Y, 0), backwardsMovingLegs);
            _engine.AddStep(nextStep);
        }

        public void EnqueueOneRotation(float rotation, LegFlags firstMovingLegs = LegFlags.LfRrCross, float liftHeight = 2)
        {
            // identity comparasion to prevent error on float.NaN
            if (rotation == 0)
            {
                return;
            }
            if (Math.Abs(rotation) > 25)
            {
                throw new ArgumentOutOfRangeException($"{nameof(rotation)} has to be between -25 and 25 degrees");
            }

            if (firstMovingLegs != LegFlags.LfRrCross && firstMovingLegs != LegFlags.RfLrCross)
            {
                throw new ArgumentException($"{nameof(firstMovingLegs)} has to be {nameof(LegFlags.RfLrCross)} or {nameof(LegFlags.LfRrCross)}");
            }
            LegFlags secondMovingLegs = firstMovingLegs == LegFlags.LfRrCross ? LegFlags.RfLrCross : LegFlags.LfRrCross;

            rotation /= 2;
            var nextStep = RelaxedStance;

            nextStep = nextStep.Copy();
            nextStep.Rotate(new Angle(-rotation), firstMovingLegs);
            nextStep.Transform(new Vector3(0, 0, liftHeight), firstMovingLegs);
            nextStep.Rotate(new Angle(rotation), secondMovingLegs);
            _engine.AddStep(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Rotate(new Angle(-rotation), firstMovingLegs);
            nextStep.Transform(new Vector3(0, 0, -liftHeight), firstMovingLegs);
            nextStep.Rotate(new Angle(rotation), secondMovingLegs);
            _engine.AddStep(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Rotate(new Angle(-rotation), secondMovingLegs);
            nextStep.Transform(new Vector3(0, 0, liftHeight), secondMovingLegs);
            nextStep.Rotate(new Angle(rotation), firstMovingLegs);
            _engine.AddStep(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Rotate(new Angle(-rotation), secondMovingLegs);
            nextStep.Transform(new Vector3(0, 0, -liftHeight), secondMovingLegs);
            nextStep.Rotate(new Angle(rotation), firstMovingLegs);
            _engine.AddStep(nextStep);
        }

        public void Dispose()
        {
            _engine.Dispose();
        }
    }
}
