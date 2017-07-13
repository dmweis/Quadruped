using System;
using DynamixelServo.Driver;

namespace DynamixelServo.Quadruped
{
    public class QuadrupedIkDriver : IDisposable
    {
        private const byte LeftFrontCoxaId = 1;
        private const byte LeftFrontFemurId = 3;
        private const byte LeftFrontTibiaId = 5;

        private const float CoxaLength = 5.3f;
        private const float FemurLength = 6.5f;
        private const float TibiaLength = 13f;

        private readonly DynamixelDriver _driver;

        private static readonly Vector3 FrontLeftCoxa = new Vector3(-6.5f, 6.5f, 0);

        public QuadrupedIkDriver(DynamixelDriver driver)
        {
            _driver = driver;
        }

        public static void MoveFrontLeftLeg(Vector3 target, QuadrupedDriver driver)
        {
            Vector3 relativeVector = target - FrontLeftCoxa;
            float targetAngle = (float)(Math.Atan2(relativeVector.X, relativeVector.Y).RadToDegree() + 45);

            float horizontalDistanceToTarget = (float)Math.Sqrt(Math.Pow(relativeVector.X, 2) + Math.Pow(relativeVector.Y, 2));
            float horizontalDistanceWithoutCoxa = horizontalDistanceToTarget - CoxaLength;
            float absoluteDistanceToTargetWithoutCoxa = (float)Math.Sqrt(Math.Pow(horizontalDistanceWithoutCoxa, 2) + Math.Pow(relativeVector.Z, 2));
            // use sss triangle solution to calculate angles
            // use law of cosinus to get angles in two corners
            float angleByTibia = (float)GetAngleByA(absoluteDistanceToTargetWithoutCoxa, FemurLength, TibiaLength);
            float angleByFemur = (float)GetAngleByA(TibiaLength, FemurLength, absoluteDistanceToTargetWithoutCoxa);
            // we have angles of the SSS trianglel. now we need angle for the servos
            float groundToTargetAngleSize = (float)Math.Atan2(horizontalDistanceWithoutCoxa,-relativeVector.Z).RadToDegree();
            if (targetAngle >= 90 || targetAngle <= -90)
            {
                // target is behind me
                // can still happen if target is right bellow me
                throw new NotSupportedException();
            }
            float femurAngle = angleByFemur + groundToTargetAngleSize;
            // these angles need to be converted to the dynamixel angles
            // in other words horizon is 150 for the dynamixel angles so we need to recalcualate them
            float correctedFemur = 240 - femurAngle - 13;
            float correctedTibia = 330 - angleByTibia - 35;
            float correctedCoxa = 150f - targetAngle;
            //return $"coxa {correctedCoxa} femur {correctedFemur} tibia {correctedTibia}";
            driver._driver.SetGoalPositionInDegrees(LeftFrontCoxaId, correctedCoxa);
            driver._driver.SetGoalPositionInDegrees(LeftFrontFemurId, correctedFemur);
            driver._driver.SetGoalPositionInDegrees(LeftFrontTibiaId, correctedTibia);
        }

        private static double GetAngleByA(double a, double b, double c)
        {
            return Math.Acos((b.ToPower(2) + c.ToPower(2) - a.ToPower(2)) / (2 * b * c)).RadToDegree();
        }

        public void Dispose()
        {
            _driver?.Dispose();
        }
    }
}
