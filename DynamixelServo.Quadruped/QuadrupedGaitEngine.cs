using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DynamixelServo.Quadruped
{
    public  abstract class QuadrupedGaitEngine : IDisposable
    {
        protected QuadrupedIkDriver Driver;
        private Task _engineTask;
        private const int UpdateFrequency = 30;

        private bool _keepRunning;
        private long _lastTick;

        private readonly Stopwatch _watch = new Stopwatch();

        protected long TimeSinceStart => _watch.ElapsedMilliseconds;
        protected long TimeSincelastTick => _watch.ElapsedMilliseconds - _lastTick;

        protected QuadrupedGaitEngine(QuadrupedIkDriver driver)
        {
            Driver = driver ?? throw new ArgumentNullException(nameof(driver));
        }

        protected void StartEngine()
        {
            if (_engineTask != null && !_engineTask.IsCompleted)
            {
                throw new InvalidOperationException("Can't start engine if it's alredy running");
            }
            _keepRunning = true;
            _engineTask = Task.Run(EngineThread);
        }

        private async Task EngineThread()
        {
            _watch.Restart();
            _lastTick = _watch.ElapsedMilliseconds;
            while (_keepRunning)
            {
                EngineSpin();
                _lastTick = _watch.ElapsedMilliseconds;
                await Task.Delay(1000 / UpdateFrequency);
            }
        }

        protected abstract void EngineSpin();

        public void SignalStop()
        {
            _keepRunning = false;
        }

        public void Stop()
        {
            _keepRunning = false;
            _engineTask.Wait();
        }

        public void DetachDriver()
        {
            if (!_engineTask.IsCompleted)
            {
                throw new InvalidOperationException("Engine has to be stopped before detaching the driver");
            }
            Driver = null;
        }

        public virtual void Dispose()
        {
            if (!_engineTask.IsCompleted)
            {
                Stop();
            }
            Driver?.Dispose();
        }
    }
}
