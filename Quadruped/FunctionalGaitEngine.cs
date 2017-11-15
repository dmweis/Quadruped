using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace Quadruped
{
    public class FunctionalGaitEngine : QuadrupedGaitEngine
    {

        private const float LegHeight = -9f;
        private const int LegDistance = 15;

        /// <summary>
        /// Forward speed
        /// </summary>
        private const int Speed = 15;
        private float NextStepLength => Speed * 0.001f * TimeSincelastTick;

        private readonly LegFlags[] _legs = { LegFlags.RightRear, LegFlags.RightFront, LegFlags.LeftRear, LegFlags.LeftFront };
        //private readonly LegFlags[] _legs = { LegFlags.LeftFront, LegFlags.LeftRear, LegFlags.RightFront, LegFlags.RightRear }; // for backwards
        //private readonly LegFlags[] _legs = { LegFlags.RightFront, LegFlags.LeftFront, LegFlags.RightRear, LegFlags.LeftRear }; // for side
        private LegPositions _lastWrittenPosition;

        private LegPositions RelaxedStance => new LegPositions
        (
            new Vector3(LegDistance, LegDistance, LegHeight),
            new Vector3(LegDistance, -LegDistance, LegHeight),
            new Vector3(-LegDistance, LegDistance, LegHeight),
            new Vector3(-LegDistance, -LegDistance, LegHeight)
        );


        public FunctionalGaitEngine(QuadrupedIkDriver driver) : base(driver)
        {
            Driver.Setup();
            var currentPosition = driver.ReadCurrentLegPositions();
            float average = (currentPosition.LeftFront.Z +
                             currentPosition.RightFront.Z +
                             currentPosition.LeftRear.Z +
                             currentPosition.RightRear.Z) / 4;
            if (average > -9)
            {
                Driver.MoveLegsSynced(RelaxedStance.Transform(new Vector3(0, 0, -LegHeight)));
                Thread.Sleep(1000);
            }
            _lastWrittenPosition = RelaxedStance;
            Driver.MoveLegsSynced(_lastWrittenPosition);
            StartEngine();
        }

        private float _rightFrontOffset;
        private float _rightRearOffset;
        private float _leftFrontOffset;
        private float _leftRearOffset;

        private readonly Vector2 _direction = new Vector2(0, 1);

        protected override void EngineSpin()
        {
            double angle = TimeSinceStart / 1000.0 * 280.0; // this determins the speed of switching legs
            double legLiftSin = Math.Sin((angle * 2).DegreeToRad());
            double swaySin = Math.Sin(angle.DegreeToRad());
            var currentLeg = _legs[(int)(angle % 360 / 90)];
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

            _lastWrittenPosition = RelaxedStance
                .Transform(new Vector3((float)(swaySin * 3) * _direction.Y, -(float)(swaySin * 3) * _direction.X, 0))
                .Transform(new Vector3(_rightFrontOffset * _direction.X, _rightFrontOffset * _direction.Y, 0), LegFlags.RightFront)
                .Transform(new Vector3(_rightRearOffset * _direction.X, _rightRearOffset * _direction.Y, 0), LegFlags.RightRear)
                .Transform(new Vector3(_leftFrontOffset * _direction.X, _leftFrontOffset * _direction.Y, 0), LegFlags.LeftFront)
                .Transform(new Vector3(_leftRearOffset * _direction.X, _leftRearOffset * _direction.Y, 0), LegFlags.LeftRear)
                .Transform(new Vector3(0, 0, (float)Math.Abs(legLiftSin) * 3), currentLeg);
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

        private LegFlags DetermineBestLeg(LegPositions currentPositions)
        {
            Vector2 rightFrontProfile = currentPositions.RightFront.ToDirectionVector2().Normal();
            Vector2 rightRearProfile = currentPositions.RightRear.ToDirectionVector2().Normal();
            Vector2 leftFrontProfile = currentPositions.LeftFront.ToDirectionVector2().Normal();
            Vector2 leftRearProfile = currentPositions.LeftRear.ToDirectionVector2().Normal();

            var relaxed = RelaxedStance;

            double rightFrontValue = AreaOfTriangle(rightRearProfile, leftFrontProfile, relaxed.LeftRear.ToDirectionVector2().Normal());
            double rightRearValue = AreaOfTriangle(rightFrontProfile, leftRearProfile, relaxed.LeftFront.ToDirectionVector2().Normal());
            double leftFrontValue = AreaOfTriangle(rightFrontProfile, leftRearProfile, relaxed.RightRear.ToDirectionVector2().Normal());
            double leftRearValue = AreaOfTriangle(rightRearProfile, leftFrontProfile, relaxed.RightFront.ToDirectionVector2().Normal());
            double highest = new double[] { rightFrontValue, rightRearValue, leftFrontValue, leftRearValue }.Max();
            if (highest == rightFrontValue)
            {
                return LegFlags.RightFront;
            }
            else if (highest == rightRearValue)
            {
                return LegFlags.RightRear;
            }
            else if (highest == leftFrontValue)
            {
                return LegFlags.LeftFront;
            }
            else if (highest == leftRearValue)
            {
                return LegFlags.LeftRear;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private static double AreaOfTriangle(Vector2 a, Vector2 b, Vector2 c)
        {
            return Math.Abs((a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y)) / 2.0);
        }
    }
}
