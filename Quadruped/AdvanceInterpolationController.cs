using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Quadruped
{
    public class AdvanceInterpolationController : IDisposable
    {
        private readonly InterpolationGaitEngine _engine;

        private const float LegHeight = -9f;
        private const int LegDistanceLongitudinal = 15;
        private const int LegDistanceLateral = 15;

        private LegFlags _lastUsedCombo = LegFlags.RfLrCross;
        private StepStage _moveStage;
        private bool _keepRunning;

        public Vector2 Direction { get; set; }
        private Vector2 _odometry = Vector2.Zero;

        public event EventHandler<QuadrupedTelemetrics> NewTelemetricsUpdate;
        public event EventHandler<Exception> GaitEngineError;

        public int Speed
        {
            get => _engine.Speed;
            set => _engine.Speed = value;
        }

        public LegPositions OriginalRelaxedStance => new LegPositions
        (
            new Vector3(LegDistanceLateral, LegDistanceLongitudinal, LegHeight),
            new Vector3(LegDistanceLateral, -LegDistanceLongitudinal, LegHeight),
            new Vector3(-LegDistanceLateral, LegDistanceLongitudinal, LegHeight),
            new Vector3(-LegDistanceLateral, -LegDistanceLongitudinal, LegHeight)
        );

        public LegPositions RelaxedStance { get; set; }

        public AdvanceInterpolationController(InterpolationGaitEngine engine)
        {
            RelaxedStance = OriginalRelaxedStance;
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _engine.NewTelemetricsUpdate += (sender, telemetrics) => NewTelemetricsUpdate?.Invoke(sender, telemetrics);
            _engine.GaitEngineError += OnGaitEngineError;
            EnqueueInitialStandup();
            _keepRunning = true;
            Task.Run(ControlLoop);
        }

        private async Task ControlLoop()
        {
            while (_keepRunning)
            {
                if (Direction.X == 0f && Direction.Y == 0f)
                {
                    if (_moveStage == StepStage.Moving)
                    {
                        EnqueueHalfStep(Direction, GetNextLegCombo(), 0.75f);
                        _moveStage = StepStage.NotMoving;
                    }
                    else
                    {
                        await Task.Delay(10);
                    }
                }
                else
                {
                    if (_moveStage == StepStage.NotMoving)
                    {
                        EnqueueHalfStep(Direction, GetNextLegCombo(), 0.75f);
                        _moveStage = StepStage.Moving;
                    }
                    else
                    {
                        EnqueueHalfStep(Direction, GetNextLegCombo());
                    }
                }
                _engine.WaitUntilCommandQueueIsEmpty();
            }
        }

        public void EnqueueHalfStep(Vector2 direction, LegFlags forwardMovingLegs = LegFlags.RfLrCross, float legShift = 1.5f, float liftHeight = 2, bool normalize = true)
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

            // Move LR and RF forward
            var nextStep = RelaxedStance
                .Transform(new Vector3(legShift * direction.X, legShift * direction.Y, liftHeight), forwardMovingLegs)
                .Transform(new Vector3(-legShift * direction.X, -legShift * direction.Y, 0), backwardsMovingLegs);
            _engine.AddStep(nextStep);

            nextStep = nextStep
                .Transform(new Vector3(legShift * direction.X, legShift * direction.Y, -liftHeight), forwardMovingLegs)
                .Transform(new Vector3(-legShift * direction.X, -legShift * direction.Y, 0), backwardsMovingLegs);
            _engine.AddStep(nextStep);

            // Move all
            nextStep = nextStep.Transform(new Vector3(-legShift * direction.X * 2, -legShift * direction.Y * 2, 0));
            _engine.AddStep(nextStep);
            _odometry += direction * legShift * 4;
        }

        private void OnGaitEngineError(object sender, Exception exception)
        {
            GaitEngineError?.Invoke(sender, exception);
        }

        private LegFlags GetNextLegCombo()
        {
            _lastUsedCombo = _lastUsedCombo == LegFlags.RfLrCross ? LegFlags.LfRrCross : LegFlags.RfLrCross;
            return _lastUsedCombo;
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
                _engine.AddStep(RelaxedStance.Transform(new Vector3(0, 0, -LegHeight)));
            }

            _engine.AddStep(RelaxedStance);
        }

        public void Start() => _engine.StartEngine();

        public void Stop()
        {
            _keepRunning = false;
            _engine.Stop();
            _engine.DisableTorqueOnMotors();
        }

        public void Dispose()
        {
            _engine.NewTelemetricsUpdate -= NewTelemetricsUpdate;
            _engine.Dispose();
        }
    }

    public enum StepStage
    {
        NotMoving,
        Moving
    }
}
