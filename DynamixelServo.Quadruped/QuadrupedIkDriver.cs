using System;
using DynamixelServo.Driver;

namespace DynamixelServo.Quadruped
{
    public class QuadrupedIkDriver : IDisposable
    {

        public static readonly LegConfiguration FrontLeft  = new LegConfiguration(1, 3, 5, 45, new Vector3(-6.5f, 6.5f, 0));
        public static readonly LegConfiguration FrontRight = new LegConfiguration(2, 4, 6, -45, new Vector3(6.5f, 6.5f, 0));
        public static readonly LegConfiguration RearLeft   = new LegConfiguration(7, 9, 11, 135, new Vector3(-6.5f, -6.5f, 0));
        public static readonly LegConfiguration RearRight  = new LegConfiguration(8, 10, 12, -135, new Vector3(6.5f, -6.5f, 0));

        private const float CoxaLength = 5.3f;
        private const float FemurLength = 6.5f;
        private const float TibiaLength = 13f;

        private readonly DynamixelDriver _driver;

        public QuadrupedIkDriver(DynamixelDriver driver)
        {
            _driver = driver;
        }

        public void MoveFrontLeftLeg(Vector3 target)
        {
            var frontLeft = CalculateIkForLeg(new Vector3(-15, 15, -13), FrontLeft.CoxaPosition, FrontLeft.AngleOffset);
            _driver.SetGoalPositionInDegrees(FrontLeft.CoxaId, frontLeft.Coxa);
            _driver.SetGoalPositionInDegrees(FrontLeft.FemurId, frontLeft.Femur);
            _driver.SetGoalPositionInDegrees(FrontLeft.TibiaId, frontLeft.Tibia);
        }

        public void MoveFrontRightLeg(Vector3 target)
        {
            var frontRight = CalculateIkForLeg(target, FrontRight.CoxaPosition, FrontRight.AngleOffset);
            _driver.SetGoalPositionInDegrees(FrontRight.CoxaId, frontRight.Coxa);
            _driver.SetGoalPositionInDegrees(FrontRight.FemurId, frontRight.Femur);
            _driver.SetGoalPositionInDegrees(FrontRight.TibiaId, frontRight.Tibia);
        }

        public void MoveRearLeftLeg(Vector3 target)
        {
            var rearLeft = CalculateIkForLeg(target, RearLeft.CoxaPosition, RearLeft.AngleOffset);
            _driver.SetGoalPositionInDegrees(RearLeft.CoxaId, rearLeft.Coxa);
            _driver.SetGoalPositionInDegrees(RearLeft.FemurId, rearLeft.Femur);
            _driver.SetGoalPositionInDegrees(RearLeft.TibiaId, rearLeft.Tibia);
        }

        public void MoveRearRightLeg(Vector3 target)
        {
            var rearRight = CalculateIkForLeg(target, RearRight.CoxaPosition, RearRight.AngleOffset);
            _driver.SetGoalPositionInDegrees(RearRight.CoxaId, rearRight.Coxa);
            _driver.SetGoalPositionInDegrees(RearRight.FemurId, rearRight.Femur);
            _driver.SetGoalPositionInDegrees(RearRight.TibiaId, rearRight.Tibia);
        }


        public void Dispose()
        {
            _driver?.Dispose();
        }

        private static LegGoalPositions CalculateIkForLeg(Vector3 target, Vector3 legPosition, float angleOffset)
        {
            Vector3 relativeVector = target - legPosition;
            float targetAngle = (float)(Math.Atan2(relativeVector.X, relativeVector.Y).RadToDegree() + angleOffset);

            float horizontalDistanceToTarget = (float)Math.Sqrt(Math.Pow(relativeVector.X, 2) + Math.Pow(relativeVector.Y, 2));
            float horizontalDistanceWithoutCoxa = horizontalDistanceToTarget - CoxaLength;
            float absoluteDistanceToTargetWithoutCoxa = (float)Math.Sqrt(Math.Pow(horizontalDistanceWithoutCoxa, 2) + Math.Pow(relativeVector.Z, 2));
            // use sss triangle solution to calculate angles
            // use law of cosinus to get angles in two corners
            float angleByTibia = (float)GetAngleByA(absoluteDistanceToTargetWithoutCoxa, FemurLength, TibiaLength);
            float angleByFemur = (float)GetAngleByA(TibiaLength, FemurLength, absoluteDistanceToTargetWithoutCoxa);
            // we have angles of the SSS trianglel. now we need angle for the servos
            float groundToTargetAngleSize = (float)Math.Atan2(horizontalDistanceWithoutCoxa, -relativeVector.Z).RadToDegree();
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
            return new LegGoalPositions(correctedCoxa, correctedFemur, correctedTibia);
        }

        private static double GetAngleByA(double a, double b, double c)
        {
            return Math.Acos((b.ToPower(2) + c.ToPower(2) - a.ToPower(2)) / (2 * b * c)).RadToDegree();
        }

    }
}
