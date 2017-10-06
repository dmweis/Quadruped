using System;
using System.IO;
using System.Numerics;
using System.Threading;

namespace DynamixelServo.Quadruped
{
    public class FunctionalQuadrupedGaitEngine : QuadrupedGaitEngine
    {

        private const float LegHeight = -11f;
        private const int LegDistance = 15;

        /// <summary>
        /// Forward speed
        /// </summary>
        private const int Speed = 15;
        private float NextStepLength => Speed * 0.001f * TimeSincelastTick;

        private readonly LegFlags[] _legs = { LegFlags.RightRear, LegFlags.RightFront, LegFlags.LeftRear, LegFlags.LeftFront };
        private LegPositions _lastWrittenPosition;

        private LegPositions RelaxedStance
        {
            get
            {
                return new LegPositions
                {
                    LeftFront = new Vector3(-LegDistance, LegDistance, LegHeight),
                    RightFront = new Vector3(LegDistance, LegDistance, LegHeight),
                    LeftRear = new Vector3(-LegDistance, -LegDistance, LegHeight),
                    RightRear = new Vector3(LegDistance, -LegDistance, LegHeight)
                }.Copy();
            }
        }


        public FunctionalQuadrupedGaitEngine(QuadrupedIkDriver driver) : base(driver)
        {
            Driver.Setup();
            var currentPosition = new LegPositions(driver);
            float average = (currentPosition.LeftFront.Z +
                             currentPosition.RightFront.Z +
                             currentPosition.LeftRear.Z +
                             currentPosition.RightRear.Z) / 4;
            if (average > -9)
            {
                Driver.MoveLegs(new LegPositions
                {
                    LeftFront = new Vector3(-LegDistance, LegDistance, 0),
                    RightFront = new Vector3(LegDistance, LegDistance, 0),
                    LeftRear = new Vector3(-LegDistance, -LegDistance, 0),
                    RightRear = new Vector3(LegDistance, -LegDistance, 0)
                });
                Thread.Sleep(1000);
            }
            _lastWrittenPosition = RelaxedStance;
            Driver.MoveLegs(_lastWrittenPosition);
            StartEngine();
        }

        private float _rightFrontOffset;
        private float _rightRearOffset;
        private float _leftFrontOffset;
        private float _leftRearOffset;

        protected override void EngineSpin()
        {
            double angle = TimeSinceStart / 1000.0 * 280.0; // this determins the speed of switching legs
            double legLiftSin = Math.Sin((angle * 2).DegreeToRad());
            double swaySin = Math.Sin(angle.DegreeToRad());
            var currentLeg = _legs[(int) (angle % 360 / 90)];
            // shift leg offsets
            // TODO refactor
            if (currentLeg == LegFlags.RightFront)
            {
                _rightFrontOffset += 3 * NextStepLength;
            }
            else
            {
                _rightFrontOffset -= NextStepLength;
            }
            if (currentLeg == LegFlags.RightRear)
            {
                _rightRearOffset += 3 * NextStepLength;
            }
            else
            {
                _rightRearOffset -= NextStepLength;
            }
            if (currentLeg == LegFlags.LeftFront)
            {
                _leftFrontOffset += 3 * NextStepLength;
            }
            else
            {
                _leftFrontOffset -= NextStepLength;
            }
            if (currentLeg == LegFlags.LeftRear)
            {
                _leftRearOffset += 3 * NextStepLength;
            }
            else
            {
                _leftRearOffset -= NextStepLength;
            }

            var newPosition = RelaxedStance;
            newPosition.Transform(new Vector3((float)(swaySin * 4), 0, 0));
            newPosition.Transform(new Vector3(0, _rightFrontOffset, 0), LegFlags.RightFront);
            newPosition.Transform(new Vector3(0, _rightRearOffset, 0), LegFlags.RightRear);
            newPosition.Transform(new Vector3(0, _leftFrontOffset, 0), LegFlags.LeftFront);
            newPosition.Transform(new Vector3(0, _leftRearOffset, 0), LegFlags.LeftRear);
            newPosition.Transform(new Vector3(0, 0, (float)Math.Abs(legLiftSin) * 3), currentLeg);
            _lastWrittenPosition = newPosition;
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
    }
}
