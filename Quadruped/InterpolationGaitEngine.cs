using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace Quadruped
{
    public class InterpolationGaitEngine : QuadrupedGaitEngine
    {
        public int Speed { get; set; } = 30;

        private float NextStepLength => Speed * 0.001f * TimeSincelastTick;

        private readonly ConcurrentQueue<LegPositions> _moves = new ConcurrentQueue<LegPositions>();

        /// <summary>
        /// Reset even is set when queue is empty and all moves are done executing
        /// </summary>
        private readonly ManualResetEventSlim _moveQueueSingal = new ManualResetEventSlim();

        private LegPositions _nextMove;
        private LegPositions _lastWrittenPosition;

        public bool IsComamndQueueEmpty => _moveQueueSingal.IsSet && _moves.IsEmpty;

        public InterpolationGaitEngine(QuadrupedIkDriver driver) : base(driver)
        {
            Driver.Setup();
            _lastWrittenPosition = Driver.ReadCurrentLegPositions();
            if (_moves.TryDequeue(out var deqeueuedLegPosition))
            {
                _nextMove = deqeueuedLegPosition;
            }
            StartEngine();
        }

        protected override void EngineSpin()
        {
            if (_lastWrittenPosition.MoveFinished(_nextMove ?? _lastWrittenPosition))
            {
                if (_moves.TryDequeue(out var deqeueuedLegPosition))
                {
                    _moveQueueSingal.Reset();
                    _nextMove = deqeueuedLegPosition;
                }
                else
                {
                    if (_moves.IsEmpty)
                    {
                        _moveQueueSingal.Set();
                    }
                    return;
                }
            }
            try
            {
                _lastWrittenPosition = _lastWrittenPosition.MoveTowards(_nextMove, NextStepLength);
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

        public void AddStep(LegPositions nextStep)
        {
            _moves.Enqueue(nextStep);
            _moveQueueSingal.Reset();
        }

        public void WaitUntilCommandQueueIsEmpty(CancellationToken cancellationToken) => _moveQueueSingal.Wait(cancellationToken);

        public void WaitUntilCommandQueueIsEmpty() => _moveQueueSingal.Wait();

        public LegPositions ReadCurrentLegPositions() => Driver.ReadCurrentLegPositions();

        public override void Dispose()
        {
            base.Dispose();
            _moveQueueSingal.Dispose();
        }
    }
}
