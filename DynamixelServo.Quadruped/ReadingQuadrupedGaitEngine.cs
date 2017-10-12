using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DynamixelServo.Quadruped
{
    class ReadingQuadrupedGaitEngine : QuadrupedGaitEngine
    {
        private const int Speed = 30;
        private float NextStepLength => Speed * 0.001f * TimeSincelastTick;

        private readonly Queue<LegPositions> _moves = new Queue<LegPositions>();

        private LegPositions _nextMove;

        private const float LegHeight = -9f;
        private const int LegDistanceLongitudinal = 17;
        private const int LegDistanceLateral = 10;
        //private const int LegDistanceLongitudinal = 15;
        //private const int LegDistanceLateral = 15;

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

        private LegPositions _lastWrittenPosition;

        public ReadingQuadrupedGaitEngine(QuadrupedIkDriver driver) : base(driver)
        {
            Driver.Setup();
            _lastWrittenPosition = Driver.ReadCurrentLegPositions();
            EnqueuePositions();
            _nextMove = _moves.Dequeue();
            Driver.DisableMotors();
            StartEngine();
        }

        protected override void EngineSpin()
        {
            if (Driver.ReadCurrentLegPositions().CloserThan(_nextMove, 2f))
            {
                if (_moves.Count > 0)
                {
                    _nextMove = _moves.Dequeue();
                }
            }
            _lastWrittenPosition = Driver.ReadCurrentLegPositions().GetShiftedTowards(_nextMove, NextStepLength);
            Console.WriteLine($"Writing {Driver.ReadCurrentLegPositions()}");
            try
            {
                //Driver.MoveLegs(_lastWrittenPosition);
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

            float liftHeight = 2;
            var nextStep = RelaxedStance;

            // Move LR and RF forward
            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(0, 2, liftHeight), LegFlags.LeftRear | LegFlags.RightFront);
            nextStep.Transform(new Vector3(0, -2, 0), LegFlags.LeftFront | LegFlags.RightRear);
            _moves.Enqueue(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(0, 2, -liftHeight), LegFlags.LeftRear | LegFlags.RightFront);
            nextStep.Transform(new Vector3(0, -2, 0), LegFlags.LeftFront | LegFlags.RightRear);
            _moves.Enqueue(nextStep);

            // Move all
            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(0, -2, 0));
            _moves.Enqueue(nextStep);

            // Move RR and LF forward
            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(0, 2, liftHeight), LegFlags.LeftFront | LegFlags.RightRear);
            nextStep.Transform(new Vector3(0, -2, 0), LegFlags.LeftRear | LegFlags.RightFront);
            _moves.Enqueue(nextStep);

            nextStep = nextStep.Copy();
            nextStep.Transform(new Vector3(0, 2, -liftHeight), LegFlags.LeftFront | LegFlags.RightRear);
            nextStep.Transform(new Vector3(0, -2, 0), LegFlags.LeftRear | LegFlags.RightFront);
            _moves.Enqueue(nextStep);
        }
    }
}
