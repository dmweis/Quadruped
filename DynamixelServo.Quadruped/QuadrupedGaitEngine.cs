using System;
using System.Diagnostics;
using System.Threading;

namespace DynamixelServo.Quadruped
{
    public  abstract class QuadrupedGaitEngine : IDisposable
    {
        protected QuadrupedIkDriver Driver;
        private readonly Thread _engineThread;
        private const int UpdateFrequency = 30;

        private bool _keepRunning;
        private long _lastTick;

        private readonly Stopwatch _watch = new Stopwatch();

        protected long TimeSinceStart => _watch.ElapsedMilliseconds;
        protected long TimeSincelastTick => _watch.ElapsedMilliseconds - _lastTick;

        protected QuadrupedGaitEngine(QuadrupedIkDriver driver)
        {
            Driver = driver ?? throw new ArgumentNullException(nameof(driver));
            _engineThread = new Thread(EngineThread)
            {
                IsBackground = true,
                Name = $"{nameof(QuadrupedGaitEngine)}Thread"
            };
        }

        protected void StartEngine()
        {
            _keepRunning = true;
            _engineThread.Start();
        }

        private void EngineThread()
        {
            _watch.Restart();
            _lastTick = _watch.ElapsedMilliseconds;
            while (_keepRunning)
            {
                EngineSpin();
                _lastTick = _watch.ElapsedMilliseconds;
                Thread.Sleep(1000 / UpdateFrequency);
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
            _engineThread.Join();
        }

        public void DetachDriver()
        {
            if (_engineThread.IsAlive)
            {
                throw new InvalidOperationException("Thread has to be endigne has to be stopped before detaching the driver");
            }
            Driver = null;
        }

        public virtual void Dispose()
        {
            if (_engineThread.IsAlive)
            {
                _engineThread.Abort();
            }
            Driver?.Dispose();
        }
    }
}
