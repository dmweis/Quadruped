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
        private static readonly LegConfiguration LeftFront  = new LegConfiguration(1, 3,  5,   45,  new Vector3(-6.5f, 6.5f, 0), -240 + FemurOffset, -330 + TibiaOffset);
        private static readonly LegConfiguration RightFront = new LegConfiguration(2, 4,  6,  -45,  new Vector3(6.5f, 6.5f, 0),   60 + FemurOffset,  -30 + TibiaOffset);
        private static readonly LegConfiguration LeftRear   = new LegConfiguration(7, 9,  11,  135, new Vector3(-6.5f, -6.5f, 0), 60 + FemurOffset,  -30 + TibiaOffset);
        private static readonly LegConfiguration RightRear  = new LegConfiguration(8, 10, 12, -135, new Vector3(6.5f, -6.5f, 0), -240 + FemurOffset, -330 + TibiaOffset);

        private static readonly byte[] Coxas  = { LeftFront.CoxaId, RightFront.CoxaId, LeftRear.CoxaId, RightRear.CoxaId };
        private static readonly byte[] Femurs = { LeftFront.FemurId, RightFront.FemurId, LeftRear.FemurId, RightRear.FemurId };
        private static readonly byte[] Tibias = { LeftFront.TibiaId, RightFront.TibiaId, LeftRear.TibiaId, RightRear.TibiaId };

        private static readonly byte[] AllMotorIds = new [] {Coxas, Femurs, Tibias}.SelectMany(x => x).ToArray();

        private const float CoxaLength = 5.3f;
        private const float FemurLength = 6.5f;
        private const float TibiaLength = 13f;

        private readonly DynamixelDriver _driver;

        public QuadrupedIkDriver(DynamixelDriver driver)
        {
            _driver = driver;
        }

        public void Setup()
        {
            foreach (var servo in AllMotorIds)
            {
                _driver.SetComplianceSlope(servo, ComplianceSlope.S64);
                _driver.SetMovingSpeed(servo, 300);
            }
        }

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

        public void RelaxedStance()
        {
            MoveLeg(new Vector3(-15,  15, -13), LeftFront);
            MoveLeg(new Vector3(15, 15, -13), RightFront);
            MoveLeg(new Vector3(-15, -15, -13), LeftRear);
            MoveLeg(new Vector3( 15, -15, -13), RightRear);
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

        public void ForwardSimple()
        {
            int ground = -13;
            int lifted = -11;
            int breakTime = 600;
            int middle = 15;
            int close = 10;
            int far = 20;
            
            MoveLeftFrontLeg(new Vector3(-close, close, ground));
            MoveRightFrontLeg(new Vector3(close, far, ground));
            MoveLeftRearLeg(new Vector3(-close, -close, ground));
            MoveRightRearLeg(new Vector3(close, -far, ground));
            Thread.Sleep(breakTime);
            MoveLeftFrontLeg(new Vector3(-close, middle, lifted));
            MoveRightFrontLeg(new Vector3(close, middle, ground));
            MoveLeftRearLeg(new Vector3(-close, -middle, ground));
            MoveRightRearLeg(new Vector3(close, -middle, lifted));
            Thread.Sleep(breakTime);
            MoveLeftFrontLeg(new Vector3(-close, far, ground));
            MoveRightFrontLeg(new Vector3(close, close, ground));
            MoveLeftRearLeg(new Vector3(-close, -far, ground));
            MoveRightRearLeg(new Vector3(close, -close, ground));
            Thread.Sleep(breakTime);
            MoveLeftFrontLeg(new Vector3(-close, middle, ground));
            MoveRightFrontLeg(new Vector3(close, middle, lifted));
            MoveLeftRearLeg(new Vector3(-close, -middle, lifted));
            MoveRightRearLeg(new Vector3(close, -middle, ground));
            Thread.Sleep(breakTime);
            MoveLeftFrontLeg(new Vector3(-close, close, ground));
            MoveRightFrontLeg(new Vector3(close, far, ground));
            MoveLeftRearLeg(new Vector3(-close, -close, ground));
            MoveRightRearLeg(new Vector3(close, -far, ground));
        }

        public void ForwardCreeper()
        {
            //int ground = -13;
            //int lifted = -11;
            int ground = -11;
            int lifted = -9;

            int timeout = 150;

            //float[] positions = {10, 13.5f, 16.5f, 20};
            float[] positions = {9, 13, 17, 21};
            int sideOffset = 10;

            ///////////// Block 1 //////////////
            MoveRightFrontLeg(new Vector3(sideOffset, positions[0], ground));
            MoveRightRearLeg(new Vector3(sideOffset, -positions[3], lifted));
            MoveLeftFrontLeg(new Vector3(-sideOffset, positions[2], ground));
            MoveLeftRearLeg(new Vector3(-sideOffset, -positions[2], ground));
            Thread.Sleep(timeout);
            MoveRightRearLeg(new Vector3(sideOffset, -positions[0], lifted));
            Thread.Sleep(timeout);
            MoveRightRearLeg(new Vector3(sideOffset, -positions[0], ground));
            Thread.Sleep(timeout);
            ///////////// Block 2 //////////////
            MoveRightFrontLeg(new Vector3(sideOffset, positions[0], lifted));
            MoveRightRearLeg(new Vector3(sideOffset, -positions[1], ground));
            MoveLeftFrontLeg(new Vector3(-sideOffset, positions[1], ground));
            MoveLeftRearLeg(new Vector3(-sideOffset, -positions[3], ground));
            Thread.Sleep(timeout);
            MoveRightFrontLeg(new Vector3(sideOffset, positions[0], lifted));
            Thread.Sleep(timeout);
            MoveRightFrontLeg(new Vector3(sideOffset, positions[3], ground));
            Thread.Sleep(timeout);
            ///////////// Block 3 //////////////
            MoveRightFrontLeg(new Vector3(sideOffset, positions[2], ground));
            MoveRightRearLeg(new Vector3(sideOffset, -positions[2], ground));
            MoveLeftFrontLeg(new Vector3(-sideOffset, positions[0], ground));
            MoveLeftRearLeg(new Vector3(-sideOffset, -positions[3], lifted));
            Thread.Sleep(timeout);
            MoveLeftRearLeg(new Vector3(-sideOffset, -positions[3], lifted));
            Thread.Sleep(timeout);
            MoveLeftRearLeg(new Vector3(-sideOffset, -positions[0], ground));
            Thread.Sleep(timeout);
            ///////////// Block 4 //////////////
            MoveRightFrontLeg(new Vector3(sideOffset, positions[1], ground));
            MoveRightRearLeg(new Vector3(sideOffset, -positions[3], ground));
            MoveLeftFrontLeg(new Vector3(-sideOffset, positions[0], lifted));
            MoveLeftRearLeg(new Vector3(-sideOffset, -positions[1], ground));
            Thread.Sleep(timeout);
            MoveLeftFrontLeg(new Vector3(-sideOffset, positions[0], lifted));
            Thread.Sleep(timeout);
            MoveLeftFrontLeg(new Vector3(-sideOffset, positions[3], ground));
            Thread.Sleep(timeout);
        }

        public void ForwardCreeperStable()
        {
            //int ground = -13;
            //int lifted = -11;
            int ground = -11;
            int lifted = -9;

            int timeout = 150;

            //float[] positions = {10, 13.5f, 16.5f, 20};
            float[] positions = { 9, 13, 17, 21 };
            int sideOffset = 10;

            ///////////// Block 1 //////////////
            MoveRightRearLeg(new Vector3(sideOffset, -positions[3], lifted));
            Thread.Sleep(timeout);
            MoveRightRearLeg(new Vector3(sideOffset, -positions[0], lifted));
            Thread.Sleep(timeout);
            MoveRightRearLeg(new Vector3(sideOffset, -positions[0], ground));
            Thread.Sleep(timeout);
            MoveRelativeCenterMass(new Vector3(0, 4, 0));
            Thread.Sleep(timeout);
            ///////////// Block 2 //////////////
            MoveRightFrontLeg(new Vector3(sideOffset, positions[0], lifted));
            Thread.Sleep(timeout);
            MoveRightFrontLeg(new Vector3(sideOffset, positions[0], lifted));
            Thread.Sleep(timeout);
            MoveRightFrontLeg(new Vector3(sideOffset, positions[3], ground));
            Thread.Sleep(timeout);
            MoveRelativeCenterMass(new Vector3(0, 4, 0));
            Thread.Sleep(timeout);
            ///////////// Block 3 //////////////
            MoveLeftRearLeg(new Vector3(-sideOffset, -positions[3], lifted));
            Thread.Sleep(timeout);
            MoveLeftRearLeg(new Vector3(-sideOffset, -positions[3], lifted));
            Thread.Sleep(timeout);
            MoveLeftRearLeg(new Vector3(-sideOffset, -positions[0], ground));
            Thread.Sleep(timeout);
            MoveRelativeCenterMass(new Vector3(0, 4, 0));
            Thread.Sleep(timeout);
            ///////////// Block 4 //////////////
            MoveLeftFrontLeg(new Vector3(-sideOffset, positions[0], lifted));
            Thread.Sleep(timeout);
            MoveLeftFrontLeg(new Vector3(-sideOffset, positions[0], lifted));
            Thread.Sleep(timeout);
            MoveLeftFrontLeg(new Vector3(-sideOffset, positions[3], ground));
            Thread.Sleep(timeout);
            MoveRelativeCenterMass(new Vector3(0, 4, 0));
            Thread.Sleep(timeout);
        }

        public void TurnRight() => Turn(20, 10, -13, -8, 200);
        public void TurnRightSlow() => Turn(16, 12, -13, -8, 150);

        public void TurnLeft() => Turn(10, 20, -13, -8, 200);
        public void TurnLeftSlow() => Turn(12, 16, -13, -8, 150);

        private void Turn(int a, int b, int ground, int lifted, int breakTime)
        {
            // initial turn
            MoveLeftFrontLeg(new Vector3(-a, b, ground));
            MoveRightFrontLeg(new Vector3(b, a, ground));
            MoveLeftRearLeg(new Vector3(-b, -a, ground));
            MoveRightRearLeg(new Vector3(a, -b, ground));
            Thread.Sleep(breakTime);
            // LeftFront and RightRear
            MoveLeftFrontLeg(new Vector3(-a, b, lifted));
            MoveRightRearLeg(new Vector3(a, -b, lifted));
            Thread.Sleep(breakTime);
            MoveLeftFrontLeg(new Vector3(-b, a, lifted));
            MoveRightRearLeg(new Vector3(b, -a, lifted));
            Thread.Sleep(breakTime);
            MoveLeftFrontLeg(new Vector3(-b, a, ground));
            MoveRightRearLeg(new Vector3(b, -a, ground));
            Thread.Sleep(breakTime);
            // RightFront and LeftRear
            MoveRightFrontLeg(new Vector3(b, a, lifted));
            MoveLeftRearLeg(new Vector3(-b, -a, lifted));
            Thread.Sleep(breakTime);
            MoveRightFrontLeg(new Vector3(a, b, lifted));
            MoveLeftRearLeg(new Vector3(-a, -b, lifted));
            Thread.Sleep(breakTime);
            MoveRightFrontLeg(new Vector3(a, b, ground));
            MoveLeftRearLeg(new Vector3(-a, -b, ground));
            Thread.Sleep(breakTime);
        }

        public void DisableMotors()
        {
            foreach (var servo in AllMotorIds)
            {
                _driver.SetTorque(servo, false);
            }
        }

        public void MoveLeftFrontLeg(Vector3 target) => MoveLeg(target, LeftFront);

        public void MoveRightFrontLeg(Vector3 target) => MoveLeg(target, RightFront);

        public void MoveLeftRearLeg(Vector3 target) => MoveLeg(target, LeftRear);

        public void MoveRightRearLeg(Vector3 target) => MoveLeg(target, RightRear);

        public Vector3 GetLeftFrontLegGoal() => GetLegGoalPosition(LeftFront);

        public Vector3 GetRightFrontLegGoal() => GetLegGoalPosition(RightFront);

        public Vector3 GetLeftRearLegGoal() => GetLegGoalPosition(LeftRear);

        public Vector3 GetRightRearLegGoal() => GetLegGoalPosition(RightRear);

        public Vector3 GetLegGoalPosition(LegConfiguration legConfig)
        {
            float coxa = _driver.GetGoalPositionInDegrees(legConfig.CoxaId);
            float femur = _driver.GetGoalPositionInDegrees(legConfig.FemurId);
            float tibia = _driver.GetGoalPositionInDegrees(legConfig.TibiaId);
            LegGoalPositions positions = new LegGoalPositions(coxa, femur, tibia);
            return CalculateFkForLeg(positions, legConfig);
        }

        public Vector3 GetLeftFrontLegPosition() => GetCurrentLegPosition(LeftFront);

        public Vector3 GetRightFrontLegPosition() => GetCurrentLegPosition(RightFront);

        public Vector3 GetLeftRearLegPosition() => GetCurrentLegPosition(LeftRear);

        public Vector3 GetRightRearLegPosition() => GetCurrentLegPosition(RightRear);

        public Vector3 GetCurrentLegPosition(LegConfiguration legConfig)
        {
            float coxa = _driver.GetPresentPositionInDegrees(legConfig.CoxaId);
            float femur = _driver.GetPresentPositionInDegrees(legConfig.FemurId);
            float tibia = _driver.GetPresentPositionInDegrees(legConfig.TibiaId);
            LegGoalPositions positions = new LegGoalPositions(coxa, femur, tibia);
            return CalculateFkForLeg(positions, legConfig);
        }

        private void MoveLeg(Vector3 target, LegConfiguration legConfig)
        {
            var legGoalPositions = CalculateIkForLeg(target, legConfig);
            _driver.SetGoalPositionInDegrees(legConfig.CoxaId, legGoalPositions.Coxa);
            _driver.SetGoalPositionInDegrees(legConfig.FemurId, legGoalPositions.Femur);
            _driver.SetGoalPositionInDegrees(legConfig.TibiaId, legGoalPositions.Tibia);
        }

        public void MoveToHeight(float height, float distance)
        {
            MoveLeg(new Vector3(-distance, distance, height), LeftFront);
            MoveLeg(new Vector3(distance, distance, height), RightFront);
            MoveLeg(new Vector3(-distance, -distance, height), LeftRear);
            MoveLeg(new Vector3(distance, -distance, height), RightRear);
        }

        public void Dispose()
        {
            _driver?.Dispose();
        }

        private static Vector3 CalculateFkForLeg(LegGoalPositions currentPsoitions, LegConfiguration legConfig)
        {
            float femurAngle = Math.Abs(currentPsoitions.Femur - Math.Abs(legConfig.FemurCorrection));
            float tibiaAngle = Math.Abs(currentPsoitions.Tibia - Math.Abs(legConfig.TibiaCorrection));
            float coxaAngle = 150 - currentPsoitions.Coxa - legConfig.AngleOffset;
            float baseX = (float)Math.Sin(coxaAngle.DegreeToRad());
            float baseY = (float)Math.Cos(coxaAngle.DegreeToRad());
            Vector3 coxaVector = new Vector3(baseX, baseY, 0) * CoxaLength;
            float femurX = (float) Math.Sin((femurAngle - 90).DegreeToRad()) * FemurLength;
            float femurY = (float) Math.Cos((femurAngle - 90).DegreeToRad()) * FemurLength;
            Vector3 femurVector = new Vector3(baseX * femurY, baseY * femurY, femurX);
            // to calculate tibia we need angle between tibia and a vertical line
            // we get this by calculating the angles formed by a horizontal line from femur, femur and part of fibia by knowing that the sum of angles is 180
            // than we just remove this from teh tibia andgle and done
            float angleForTibiaVector = tibiaAngle - (180 - 90 - (femurAngle - 90));
            float tibiaX = (float) Math.Sin(angleForTibiaVector.DegreeToRad()) * TibiaLength;
            float tibiaY = (float) Math.Cos(angleForTibiaVector.DegreeToRad()) * TibiaLength;
            Vector3 tibiaVector = new Vector3(baseX * tibiaX, baseY * tibiaX, -tibiaY);
            return legConfig.CoxaPosition + coxaVector + femurVector + tibiaVector;
        }
        
        private static LegGoalPositions CalculateIkForLeg(Vector3 target, LegConfiguration legConfig)
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
            return new LegGoalPositions(correctedCoxa, correctedFemur, correctedTibia);
        }

        private static double GetAngleByA(double a, double b, double c)
        {
            return Math.Acos((b.ToPower(2) + c.ToPower(2) - a.ToPower(2)) / (2 * b * c)).RadToDegree();
        }

    }
}
