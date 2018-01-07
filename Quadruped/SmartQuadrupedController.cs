using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Quadruped
{
    public class SmartQuadrupedController : IDisposable
    {
        private const int InterpolationUpdateFrequency = 30;

        private readonly QuadrupedIkDriver _driver;
        private readonly int _updateDelay = 1000 / InterpolationUpdateFrequency;

        private const float LegHeight = -9f;
        private const int LegDistanceLongitudinal = 15;
        private const int LegDistanceLateral = 15;

        public LegPositions OriginalRelaxedStance => new LegPositions
        (
            new Vector3(LegDistanceLateral, LegDistanceLongitudinal, LegHeight),
            new Vector3(LegDistanceLateral, -LegDistanceLongitudinal, LegHeight),
            new Vector3(-LegDistanceLateral, LegDistanceLongitudinal, LegHeight),
            new Vector3(-LegDistanceLateral, -LegDistanceLongitudinal, LegHeight)
        );

        public LegPositions RelaxedStance { get; set; }
        public int Speed { get; set; } = 60;
        public Vector2 Direction { get; set; }

        private LegPositions _lastWrittenPosition;
        private Vector2 _odometry = Vector2.Zero;
        private LegFlags _lastUsedCombo = LegFlags.RfLrCross;
        private bool _keepRunning = true;
        private bool _moving;

        public SmartQuadrupedController(QuadrupedIkDriver driver)
        {
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
            RelaxedStance = OriginalRelaxedStance;
            _driver.Setup();
            _lastWrittenPosition = _driver.ReadCurrentLegPositions();
            Task.Run(MainControllerLoop);
        }

        private async Task MainControllerLoop()
        {
            await ExecuteMove(RelaxedStance);
            while (_keepRunning)
            {
                if (Direction.IsZero())
                {
                    if (_moving)
                    {
                        await StepToRelaxed(GetNextLegCombo());
                        _moving = false;
                    }
                    else
                    {
                        await Task.Delay(_updateDelay);
                    }
                }
                else
                {
                    await ExecuteStep(Direction, GetNextLegCombo());
                    _moving = true;
                }
                Console.WriteLine(_odometry);
            }
            _driver.DisableMotors();
        }

        private async Task StepToRelaxed(LegFlags legsToAlign, float liftHeight = 2)
        {
            if (legsToAlign != LegFlags.LfRrCross && legsToAlign != LegFlags.RfLrCross)
            {
                throw new ArgumentException($"{nameof(legsToAlign)} has to be {nameof(LegFlags.RfLrCross)} or {nameof(LegFlags.LfRrCross)}");
            }
            await ExecuteMove(RelaxedStance
                .Transform(new Vector3(0, 0, liftHeight), legsToAlign));
            await ExecuteMove(RelaxedStance);
        }

        private async Task ExecuteStep(Vector2 direction, LegFlags forwardMovingLegs, float distance = 6f, float liftHeight = 2)
        {
            // identity comparasion to prevent error on float.NaN
            if (!direction.IsZero())
            {
                direction = direction.Normal();
            }
            if (forwardMovingLegs != LegFlags.LfRrCross && forwardMovingLegs != LegFlags.RfLrCross)
            {
                throw new ArgumentException($"{nameof(forwardMovingLegs)} has to be {nameof(LegFlags.RfLrCross)} or {nameof(LegFlags.LfRrCross)}");
            }
            LegFlags backwardsMovingLegs = forwardMovingLegs == LegFlags.LfRrCross ? LegFlags.RfLrCross : LegFlags.LfRrCross;
            var legShift = distance / 4;

            // Lift
            var nextStep = RelaxedStance
                .Transform(new Vector3(legShift * direction.X, legShift * direction.Y, liftHeight), forwardMovingLegs)
                .Transform(new Vector3(-legShift * direction.X, -legShift * direction.Y, 0), backwardsMovingLegs);
            await ExecuteMove(nextStep);

            // Lower
            nextStep = nextStep
                .Transform(new Vector3(legShift * direction.X, legShift * direction.Y, -liftHeight), forwardMovingLegs)
                .Transform(new Vector3(-legShift * direction.X, -legShift * direction.Y, 0), backwardsMovingLegs);
            await ExecuteMove(nextStep);

            // Move all
            nextStep = nextStep.Transform(new Vector3(-legShift * direction.X * 2, -legShift * direction.Y * 2, 0));
            await ExecuteMove(nextStep);

            _odometry += direction * distance;
        }

        private async Task ExecuteMove(LegPositions targetPosition)
        {
            while (!_lastWrittenPosition.MoveFinished(targetPosition))
            {
                _lastWrittenPosition = _lastWrittenPosition.MoveTowards(targetPosition, Speed * 0.001f * _updateDelay);
                _driver.MoveLegsSynced(_lastWrittenPosition);
                await Task.Delay(_updateDelay);
            }
        }

        private LegFlags GetNextLegCombo()
        {
            _lastUsedCombo = _lastUsedCombo == LegFlags.RfLrCross ? LegFlags.LfRrCross : LegFlags.RfLrCross;
            return _lastUsedCombo;
        }

        public void Stop()
        {
            _keepRunning = false;
        }

        public void Dispose()
        {
            _driver.Dispose();
        }
    }
}
