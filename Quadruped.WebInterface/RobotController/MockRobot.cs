using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Timers;
using Quadruped.Driver;

namespace Quadruped.WebInterface.RobotController
{
    public class MockRobot : IRobot
    {
        public event EventHandler<QuadrupedTelemetrics> NewTelemetricsUpdate;

        public Vector2 Direction { get; set; }

        public float Rotation { get; set; }

        public RobotConfig GaitConfiguration { get; set; } = new RobotConfig();

        public Vector3 RelaxedStance { get; private set; }

        public Rotation RelaxedStanceRotation { get; private set; } = new Rotation();

        private readonly Timer _fakeTelemetricsTimer;
        private readonly Random _rng = new Random();

        public MockRobot()
        {
            _fakeTelemetricsTimer = new Timer
            {
                AutoReset = true,
                Enabled = true,
                Interval = 2000
            };
            _fakeTelemetricsTimer.Elapsed += (sender, args) =>
            {
                var servos = new List<ServoTelemetrics>();
                for (int i = 0; i < _rng.Next(1, 12); i++)
                {
                    servos.Add(new ServoTelemetrics
                    {
                        Id = i,
                        Load = _rng.Next(0, 200),
                        Temperature = _rng.Next(20, 50),
                        Voltage = (float)_rng.NextDouble() * 5 + 9
                    });
                }
                NewTelemetricsUpdate?.Invoke(this, new QuadrupedTelemetrics { ServoTelemetrics = servos });
            };
        }

        public void StartRobot()
        {
        }

        public void UpdateAboluteRelaxedStance(Vector3 transform, Rotation rotation)
        {
            RelaxedStance = transform;
            RelaxedStanceRotation = rotation;
        }

        public Task DisableMotors()
        {
            return Task.CompletedTask;
        }
    }
}
