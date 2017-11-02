using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using DynamixelServo.Driver;

namespace DynamixelServo.Quadruped
{
    public class QuadrupedIkDriver : IDisposable
    {
        // angles by which the legs are ofcenter from the motor centers
        private const int FemurOffset = 13;
        private const int TibiaOffset = 35;

        // Correction numbers are set up so that if you add them to the angle the motor center will bo pointing to the desired angle
        // the offset for each axis is than added to compensate for the shape of the legs
        private static readonly LegConfiguration LeftFront = new LegConfiguration(1, 3, 5, 45, new Vector3(-6.5f, 6.5f, 0), -240 + FemurOffset, -330 + TibiaOffset);
        private static readonly LegConfiguration RightFront = new LegConfiguration(2, 4, 6, -45, new Vector3(6.5f, 6.5f, 0), 60 + FemurOffset, -30 + TibiaOffset);
        private static readonly LegConfiguration LeftRear = new LegConfiguration(7, 9, 11, 135, new Vector3(-6.5f, -6.5f, 0), 60 + FemurOffset, -30 + TibiaOffset);
        private static readonly LegConfiguration RightRear = new LegConfiguration(8, 10, 12, -135, new Vector3(6.5f, -6.5f, 0), -240 + FemurOffset, -330 + TibiaOffset);

        private static readonly byte[] Coxas = { LeftFront.CoxaId, RightFront.CoxaId, LeftRear.CoxaId, RightRear.CoxaId };
        private static readonly byte[] Femurs = { LeftFront.FemurId, RightFront.FemurId, LeftRear.FemurId, RightRear.FemurId };
        private static readonly byte[] Tibias = { LeftFront.TibiaId, RightFront.TibiaId, LeftRear.TibiaId, RightRear.TibiaId };

        private static readonly byte[] AllMotorIds = new[] { Coxas, Femurs, Tibias }.SelectMany(x => x).ToArray();

        private const float CoxaLength = 5.3f;
        private const float FemurLength = 6.5f;
        private const float TibiaLength = 13f;

        private readonly DynamixelDriver _driver;

        public QuadrupedIkDriver(DynamixelDriver driver)
        {
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
        }

        public void Setup()
        {
            lock (_driver.SyncLock)
            {
                foreach (var servo in AllMotorIds)
                {
                    _driver.SetComplianceSlope(servo, ComplianceSlope.S64);
                    _driver.SetMovingSpeed(servo, 300);
                }
            }
        }

        [Obsolete("This method is deprecated and will be removed at some point")]
        public void StandUpfromGround()
        {
            float average = (GetLeftFrontLegPosition().Z +
                GetRightFrontLegPosition().Z +
                GetLeftRearLegPosition().Z +
                GetRightRearLegPosition().Z) / 4;
            if (average > -9)
            {
                MoveLeftFrontLeg(new Vector3(-15, 15, 0));
                MoveRightFrontLeg(new Vector3(15, 15, 0));
                MoveLeftRearLeg(new Vector3(-15, -15, 0));
                MoveRightRearLeg(new Vector3(15, -15, 0));
                Thread.Sleep(1000);
            }
            MoveLeftFrontLeg(new Vector3(-15, 15, -13));
            MoveRightFrontLeg(new Vector3(15, 15, -13));
            MoveLeftRearLeg(new Vector3(-15, -15, -13));
            MoveRightRearLeg(new Vector3(15, -15, -13));
            Thread.Sleep(500);
        }

        public void MoveRelativeCenterMass(Vector3 transform)
        {
            Vector3 leftFrontPosition = GetLeftFrontLegGoal();
            Vector3 rightFrontPosition = GetRightFrontLegGoal();
            Vector3 leftRearPosition = GetLeftRearLegGoal();
            Vector3 rightRearPosition = GetRightRearLegGoal();
            MoveLeftFrontLeg(leftFrontPosition - transform);
            MoveRightFrontLeg(rightFrontPosition - transform);
            MoveLeftRearLeg(leftRearPosition - transform);
            MoveRightRearLeg(rightRearPosition - transform);
        }

        public void MoveAbsoluteCenterMass(Vector3 transform, float legDistance, float height)
        {
            MoveLeftFrontLeg(new Vector3(-legDistance, legDistance, height) - transform);
            MoveRightFrontLeg(new Vector3(legDistance, legDistance, height) - transform);
            MoveLeftRearLeg(new Vector3(-legDistance, -legDistance, height) - transform);
            MoveRightRearLeg(new Vector3(legDistance, -legDistance, height) - transform);
        }

        public void DisableMotors()
        {
            lock (_driver.SyncLock)
            {
                foreach (var servo in AllMotorIds)
                {
                    _driver.SetTorque(servo, false);
                }
            }
        }

        public void MoveLeftFrontLeg(Vector3 target) => MoveLeg(target, LeftFront);

        public void MoveRightFrontLeg(Vector3 target) => MoveLeg(target, RightFront);

        public void MoveLeftRearLeg(Vector3 target) => MoveLeg(target, LeftRear);

        public void MoveRightRearLeg(Vector3 target) => MoveLeg(target, RightRear);

        public void MoveLeg(Vector3 target, LegFlags legs = LegFlags.All)
        {
            if (legs == LegFlags.LeftFront)
            {
                MoveLeftFrontLeg(target);
            }
            if (legs == LegFlags.RightFront)
            {
                MoveRightFrontLeg(target);
            }
            if (legs == LegFlags.LeftRear)
            {
                MoveLeftRearLeg(target);
            }
            if (legs == LegFlags.RightRear)
            {
                MoveRightRearLeg(target);
            }
            else
            {
                throw new InvalidOperationException("Moving multiple legs to the same position is not advisable");
            }
        }

        public LegPositions ReadCurrentLegPositions()
        {
            return new LegPositions
            {
                LeftFront = GetLeftFrontLegPosition(),
                RightFront = GetRightFrontLegPosition(),
                LeftRear = GetLeftRearLegPosition(),
                RightRear = GetRightRearLegPosition()
            };
        }

        public Vector3 GetLeftFrontLegGoal() => GetLegGoalPosition(LeftFront);

        public Vector3 GetRightFrontLegGoal() => GetLegGoalPosition(RightFront);

        public Vector3 GetLeftRearLegGoal() => GetLegGoalPosition(LeftRear);

        public Vector3 GetRightRearLegGoal() => GetLegGoalPosition(RightRear);

        public Vector3 GetLeftFrontLegPosition() => GetCurrentLegPosition(LeftFront);

        public Vector3 GetRightFrontLegPosition() => GetCurrentLegPosition(RightFront);

        public Vector3 GetLeftRearLegPosition() => GetCurrentLegPosition(LeftRear);

        public Vector3 GetRightRearLegPosition() => GetCurrentLegPosition(RightRear);

        public void MoveToHeight(float height, float distance)
        {
            MoveLeg(new Vector3(-distance, distance, height), LeftFront);
            MoveLeg(new Vector3(distance, distance, height), RightFront);
            MoveLeg(new Vector3(-distance, -distance, height), LeftRear);
            MoveLeg(new Vector3(distance, -distance, height), RightRear);
        }

        public void Dispose()
        {
            lock (_driver.SyncLock)
            {
                _driver.Dispose();
            }
        }

        private Vector3 GetLegGoalPosition(LegConfiguration legConfig)
        {
            MotorGoalPositions positions;
            lock (_driver.SyncLock)
            {
                float coxa = _driver.GetGoalPositionInDegrees(legConfig.CoxaId);
                float femur = _driver.GetGoalPositionInDegrees(legConfig.FemurId);
                float tibia = _driver.GetGoalPositionInDegrees(legConfig.TibiaId);
                positions = new MotorGoalPositions(coxa, femur, tibia);
            }
            return CalculateFkForLeg(positions, legConfig);
        }

        private Vector3 GetCurrentLegPosition(LegConfiguration legConfig)
        {
            MotorGoalPositions positions;
            lock (_driver.SyncLock)
            {
                float coxa = _driver.GetPresentPositionInDegrees(legConfig.CoxaId);
                float femur = _driver.GetPresentPositionInDegrees(legConfig.FemurId);
                float tibia = _driver.GetPresentPositionInDegrees(legConfig.TibiaId);
                positions = new MotorGoalPositions(coxa, femur, tibia);
            }
            return CalculateFkForLeg(positions, legConfig);
        }

        public void MoveLegs(LegPositions position)
        {
            MoveLeftFrontLeg(position.LeftFront);
            MoveRightFrontLeg(position.RightFront);
            MoveLeftRearLeg(position.LeftRear);
            MoveRightRearLeg(position.RightRear);
        }

        public void MoveLegsSynced(LegPositions position)
        {
            if (position == null) throw new ArgumentNullException(nameof(position));
            var rightFrontLegGoalPositions = CalculateIkForLeg(position.RightFront, RightFront);
            var rightRearLegGoalPositions = CalculateIkForLeg(position.RightRear, RightRear);
            var leftFrontLegGoalPositions = CalculateIkForLeg(position.LeftFront, LeftFront);
            var leftRearLegGoalPositions = CalculateIkForLeg(position.LeftRear, LeftRear);
            byte[] servoIds = new byte[12];
            float[] servoGoals = new float[12];

            // Pair motor ids with their respective goal positions
            servoIds[0] = RightFront.CoxaId;
            servoGoals[0] = rightFrontLegGoalPositions.Coxa;
            servoIds[1] = RightFront.FemurId;
            servoGoals[1] = rightFrontLegGoalPositions.Femur;
            servoIds[2] = RightFront.TibiaId;
            servoGoals[2] = rightFrontLegGoalPositions.Tibia;

            servoIds[3] = RightRear.CoxaId;
            servoGoals[3] = rightRearLegGoalPositions.Coxa;
            servoIds[4] = RightRear.FemurId;
            servoGoals[4] = rightRearLegGoalPositions.Femur;
            servoIds[5] = RightRear.TibiaId;
            servoGoals[5] = rightRearLegGoalPositions.Tibia;

            servoIds[6] = LeftFront.CoxaId;
            servoGoals[6] = leftFrontLegGoalPositions.Coxa;
            servoIds[7] = LeftFront.FemurId;
            servoGoals[7] = leftFrontLegGoalPositions.Femur;
            servoIds[8] = LeftFront.TibiaId;
            servoGoals[8] = leftFrontLegGoalPositions.Tibia;

            servoIds[9] = LeftRear.CoxaId;
            servoGoals[9] = leftRearLegGoalPositions.Coxa;
            servoIds[10] = LeftRear.FemurId;
            servoGoals[10] = leftRearLegGoalPositions.Femur;
            servoIds[11] = LeftRear.TibiaId;
            servoGoals[11] = leftRearLegGoalPositions.Tibia;

            lock (_driver.SyncLock)
            {
                _driver.GroupSyncSetGoalPositionInDegrees(servoIds, servoGoals);
            }
        }

        private void MoveLeg(Vector3 target, LegConfiguration legConfig)
        {
            var legGoalPositions = CalculateIkForLeg(target, legConfig);
            lock (_driver.SyncLock)
            {
                _driver.SetGoalPositionInDegrees(legConfig.CoxaId, legGoalPositions.Coxa);
                _driver.SetGoalPositionInDegrees(legConfig.FemurId, legGoalPositions.Femur);
                _driver.SetGoalPositionInDegrees(legConfig.TibiaId, legGoalPositions.Tibia);
            }
        }

        private static Vector3 CalculateFkForLeg(MotorGoalPositions currentPsoitions, LegConfiguration legConfig)
        {
            float femurAngle = Math.Abs(currentPsoitions.Femur - Math.Abs(legConfig.FemurCorrection));
            float tibiaAngle = Math.Abs(currentPsoitions.Tibia - Math.Abs(legConfig.TibiaCorrection));
            float coxaAngle = 150 - currentPsoitions.Coxa - legConfig.AngleOffset;
            float baseX = (float)Math.Sin(coxaAngle.DegreeToRad());
            float baseY = (float)Math.Cos(coxaAngle.DegreeToRad());
            Vector3 coxaVector = new Vector3(baseX, baseY, 0) * CoxaLength;
            float femurX = (float)Math.Sin((femurAngle - 90).DegreeToRad()) * FemurLength;
            float femurY = (float)Math.Cos((femurAngle - 90).DegreeToRad()) * FemurLength;
            Vector3 femurVector = new Vector3(baseX * femurY, baseY * femurY, femurX);
            // to calculate tibia we need angle between tibia and a vertical line
            // we get this by calculating the angles formed by a horizontal line from femur, femur and part of fibia by knowing that the sum of angles is 180
            // than we just remove this from teh tibia andgle and done
            float angleForTibiaVector = tibiaAngle - (180 - 90 - (femurAngle - 90));
            float tibiaX = (float)Math.Sin(angleForTibiaVector.DegreeToRad()) * TibiaLength;
            float tibiaY = (float)Math.Cos(angleForTibiaVector.DegreeToRad()) * TibiaLength;
            Vector3 tibiaVector = new Vector3(baseX * tibiaX, baseY * tibiaX, -tibiaY);
            return legConfig.CoxaPosition + coxaVector + femurVector + tibiaVector;
        }

        private static MotorGoalPositions CalculateIkForLeg(Vector3 target, LegConfiguration legConfig)
        {
            Vector3 relativeVector = target - legConfig.CoxaPosition;
            float targetAngle = (float)(Math.Atan2(relativeVector.X, relativeVector.Y).RadToDegree() + legConfig.AngleOffset);

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
                throw new NotSupportedException($"Target angle is {targetAngle}");
            }
            float femurAngle = angleByFemur + groundToTargetAngleSize;
            // these angles need to be converted to the dynamixel angles
            // in other words horizon is 150 for the dynamixel angles so we need to recalcualate them
            float correctedFemur = Math.Abs(legConfig.FemurCorrection + femurAngle);
            float correctedTibia = Math.Abs(legConfig.TibiaCorrection + angleByTibia);
            float correctedCoxa = 150f - targetAngle;
            return new MotorGoalPositions(correctedCoxa, correctedFemur, correctedTibia);
        }

        private static double GetAngleByA(double a, double b, double c)
        {
            return Math.Acos((b.ToPower(2) + c.ToPower(2) - a.ToPower(2)) / (2 * b * c)).RadToDegree();
        }

    }
}
