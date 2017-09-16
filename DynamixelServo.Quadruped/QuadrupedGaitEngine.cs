using System;
using System.Threading;

namespace DynamixelServo.Quadruped
{
    abstract class QuadrupedGaitEngine : IDisposable
    {
        private readonly QuadrupedIkDriver _driver;
        private readonly Thread _engineThread;
        private const int UpdateFrequency = 30;

        private bool _keepRunning = true;
        private int _initialTime;
        private int _lastTick;

        protected int TimeSinceStart => Environment.TickCount - _initialTime;
        protected int TimeSincelastTick => Environment.TickCount - _lastTick;

        protected QuadrupedGaitEngine(QuadrupedIkDriver driver)
        {
            _driver = driver;
            _engineThread = new Thread(EngineThread)
            {
                IsBackground = true,
                Name = $"{nameof(QuadrupedGaitEngine)}Thread"
            };
            _engineThread.Start();
        }

        private void EngineThread()
        {
            _initialTime = Environment.TickCount;
            while (_keepRunning)
            {
                _lastTick = Environment.TickCount;
                EngineSpin();
                Thread.Sleep(1000 / UpdateFrequency);
            }
        }

        protected abstract void EngineSpin();

        protected void SignalStop()
        {
            _keepRunning = false;
        }

        protected void Stop()
        {
            _keepRunning = false;
            _engineThread.Join();
        }

        public void Dispose()
        {
            if (_engineThread.IsAlive)
            {
                _engineThread.Abort();
            }
            _driver?.Dispose();
        }
    }
}
