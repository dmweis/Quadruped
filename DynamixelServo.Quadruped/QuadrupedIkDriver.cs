using System;
using DynamixelServo.Driver;

namespace DynamixelServo.Quadruped
{
    public class QuadrupedIkDriver : IDisposable
    {
        private readonly DynamixelDriver _driver;

        private static readonly Vector2 FrontLeftCoxa = new Vector2(-7, 6);

        public QuadrupedIkDriver(DynamixelDriver driver)
        {
            _driver = driver;
        }

        public void MoveFrontLeftLeg(Vector3 target)
        {
            Vector2 relativeVector = (Vector2) target - FrontLeftCoxa;
            Angle targetAngle = (Angle) (Math.Atan2(relativeVector.X, relativeVector.Y).RadToDegree() + 45);
            ushort coxaPos = targetAngle.ToDynamixelUnits();
            return coxaPos;
        }

        public void Dispose()
        {
            _driver?.Dispose();
        }
    }
}
