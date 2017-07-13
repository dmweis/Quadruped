using System;
using DynamixelServo.Driver;
using System.Threading;

namespace DynamixelServo.Quadruped
{
    public class QuadrupedDriver : IDisposable
    {
        private readonly DynamixelDriver _driver;
        private const int TurnAngle = 20;
        private const int RelaxedFemur = -20;
        private const int RelaxedTibia = 60;
        public const int BreakTime = 250;
        private const int LiftedFemur = -80;
        private const int LiftedTibia = 90;

        private static readonly byte[] Coxas = { 1, 2, 7, 8 };
        private static readonly byte[] Femurs = { 3, 4, 9, 10 };
        private static readonly byte[] Tibias = { 5, 6, 11, 12 };

        public QuadrupedDriver(DynamixelDriver driver)
        {
            _driver = driver;
        }

        public void SetupAndStance()
        {
            foreach (var servo in Coxas)
            {
                _driver.SetComplianceSlope(servo, ComplianceSlope.S32);
                _driver.SetMovingSpeed(servo, 300);
            }
            foreach (var servo in Femurs)
            {
                _driver.SetComplianceSlope(servo, ComplianceSlope.S32);
                _driver.SetMovingSpeed(servo, 300);
            }
            foreach (var servo in Tibias)
            {
                _driver.SetComplianceSlope(servo, ComplianceSlope.S32);
                _driver.SetMovingSpeed(servo, 300);
            }
            SetRobot(0, RelaxedFemur, RelaxedTibia);
        }

        public void Stance()
        {
            SetRobot(0, RelaxedFemur, RelaxedTibia);
        }

        public void TurnLeft()
        {

            SetLeg1(TurnAngle, RelaxedFemur, RelaxedTibia);
            SetLeg2(-TurnAngle, RelaxedFemur, RelaxedTibia);
            SetLeg7(-TurnAngle, RelaxedFemur, RelaxedTibia);
            SetLeg8(TurnAngle, RelaxedFemur, RelaxedTibia);
            Thread.Sleep(BreakTime);
            // leg 1 and 8 move
            SetLeg1(TurnAngle, LiftedFemur, LiftedTibia);
            SetLeg8(TurnAngle, LiftedFemur, LiftedTibia);
            Thread.Sleep(BreakTime);
            SetLeg1(-TurnAngle, LiftedFemur, LiftedTibia);
            SetLeg8(-TurnAngle, LiftedFemur, LiftedTibia);
            Thread.Sleep(BreakTime);
            SetLeg1(-TurnAngle, RelaxedFemur, RelaxedTibia);
            SetLeg8(-TurnAngle, RelaxedFemur, RelaxedTibia);
            Thread.Sleep(BreakTime);
            // leg 2 and 7 move
            SetLeg2(-TurnAngle, LiftedFemur, LiftedTibia);
            SetLeg7(-TurnAngle, LiftedFemur, LiftedTibia);
            Thread.Sleep(BreakTime);
            SetLeg2(TurnAngle, LiftedFemur, LiftedTibia);
            SetLeg7(TurnAngle, LiftedFemur, LiftedTibia);
            Thread.Sleep(BreakTime);
            SetLeg2(TurnAngle, RelaxedFemur, RelaxedTibia);
            SetLeg7(TurnAngle, RelaxedFemur, RelaxedTibia);
            Thread.Sleep(BreakTime);
        }

        public void TurnRight()
        {
            SetLeg1(-TurnAngle, RelaxedFemur, RelaxedTibia);
            SetLeg2(TurnAngle, RelaxedFemur, RelaxedTibia);
            SetLeg7(TurnAngle, RelaxedFemur, RelaxedTibia);
            SetLeg8(-TurnAngle, RelaxedFemur, RelaxedTibia);
            Thread.Sleep(BreakTime);
            // leg 1 and 8 move
            SetLeg1(-TurnAngle, LiftedFemur, RelaxedTibia);
            SetLeg8(-TurnAngle, LiftedFemur, RelaxedTibia);
            Thread.Sleep(50);
            SetLeg1(-TurnAngle, LiftedFemur, LiftedTibia);
            SetLeg8(-TurnAngle, LiftedFemur, LiftedTibia);
            Thread.Sleep(BreakTime);
            SetLeg1(TurnAngle, LiftedFemur, LiftedTibia);
            SetLeg8(TurnAngle, LiftedFemur, LiftedTibia);
            Thread.Sleep(BreakTime);
            SetLeg1(TurnAngle, RelaxedFemur, RelaxedTibia);
            SetLeg8(TurnAngle, RelaxedFemur, RelaxedTibia);
            Thread.Sleep(BreakTime);
            // leg 2 and 7 move
            SetLeg2(TurnAngle, LiftedFemur, LiftedTibia);
            SetLeg7(TurnAngle, LiftedFemur, LiftedTibia);
            Thread.Sleep(BreakTime);
            SetLeg2(-TurnAngle, LiftedFemur, LiftedTibia);
            SetLeg7(-TurnAngle, LiftedFemur, LiftedTibia);
            Thread.Sleep(BreakTime);
            SetLeg2(-TurnAngle, RelaxedFemur, RelaxedTibia);
            SetLeg7(-TurnAngle, RelaxedFemur, RelaxedTibia);
            Thread.Sleep(BreakTime);
        }

        public void SetRobot(float coxa, float femur, float tibia)
        {
            SetLeg1(coxa, femur, tibia);
            SetLeg2(coxa, femur, tibia);
            SetLeg7(coxa, femur, tibia);
            SetLeg8(coxa, femur, tibia);
        }

        public void SetLeg1(float coxa, float femur, float tibia)
        {
            int coxaUnits = DynamixelDriver.DegreesToUnits(150 - coxa);
            int femurUnits = DynamixelDriver.DegreesToUnits(150 + femur);
            int tibiaUnits = DynamixelDriver.DegreesToUnits(150 + tibia);
            // coxa
            _driver.SetGoalPosition(1, (ushort)coxaUnits);
            // femur
            _driver.SetGoalPosition(3, (ushort)femurUnits);
            // tibia
            _driver.SetGoalPosition(5, (ushort)tibiaUnits);
        }

        public void SetLeg2(float coxa, float femur, float tibia)
        {
            int coxaUnits = DynamixelDriver.DegreesToUnits(150 + coxa);
            int femurUnits = DynamixelDriver.DegreesToUnits(150 - femur);
            int tibiaUnits = DynamixelDriver.DegreesToUnits(150 - tibia);
            // coxa
            _driver.SetGoalPosition(2, (ushort)coxaUnits);
            // femur
            _driver.SetGoalPosition(4, (ushort)femurUnits);
            // tibia
            _driver.SetGoalPosition(6, (ushort)tibiaUnits);
        }

        public void SetLeg7(float coxa, float femur, float tibia)
        {
            int coxaUnits = DynamixelDriver.DegreesToUnits(150 + coxa);
            int femurUnits = DynamixelDriver.DegreesToUnits(150 - femur);
            int tibiaUnits = DynamixelDriver.DegreesToUnits(150 - tibia);
            // coxa
            _driver.SetGoalPosition(7, (ushort)coxaUnits);
            // femur
            _driver.SetGoalPosition(9, (ushort)femurUnits);
            // tibia
            _driver.SetGoalPosition(11, (ushort)tibiaUnits);
        }

        public void SetLeg8(float coxa, float femur, float tibia)
        {
            int coxaUnits = DynamixelDriver.DegreesToUnits(150 - coxa);
            int femurUnits = DynamixelDriver.DegreesToUnits(150 + femur);
            int tibiaUnits = DynamixelDriver.DegreesToUnits(150 + tibia);
            // coxa
            _driver.SetGoalPosition(8, (ushort)coxaUnits);
            // femur
            _driver.SetGoalPosition(10, (ushort)femurUnits);
            // tibia
            _driver.SetGoalPosition(12, (ushort)tibiaUnits);
        }

        public void Dispose()
        {
            _driver?.Dispose();
        }
    }
}
