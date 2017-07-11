using System.Threading;
using DynamixelServo.Driver;

namespace DynamixelServo.TestConsole
{
    class Quadropod
    {
        private const int TurnAngle = 20;
        private const int RelaxedFemur = -20;
        private const int RelaxedTibia = 60;
        public const int BreakTime = 500;
        private const int LiftedFemur = -80;
        private const int LiftedTibia = 90;

        private static readonly byte[] Coxas = {1, 2, 7, 8};
        private static readonly byte[] Femurs = {3, 4, 9, 10};
        private static readonly byte[] Tibias = {5, 6, 11, 12};

        public static void SetupAndStance(DynamixelDriver driver)
        {
            foreach (var servo in Coxas)
            {
                driver.SetComplianceSlope(servo, ComplianceSlope.Default);
                driver.SetMovingSpeed(servo, 200);
            }
            foreach (var servo in Femurs)
            {
                driver.SetComplianceSlope(servo, ComplianceSlope.Default);
                driver.SetMovingSpeed(servo, 200);
            }
            foreach (var servo in Tibias)
            {
                driver.SetComplianceSlope(servo, ComplianceSlope.Default);
                driver.SetMovingSpeed(servo, 200);
            }
            SetRobot(0, RelaxedFemur, RelaxedTibia, driver);
            
        }

        public static void Stance(DynamixelDriver driver)
        {
            SetRobot(0, RelaxedFemur, RelaxedTibia, driver);
        }

        public static void TurnLeft(DynamixelDriver driver)
        {
            
            SetLeg1(TurnAngle, RelaxedFemur, RelaxedTibia, driver);
            SetLeg2(-TurnAngle, RelaxedFemur, RelaxedTibia, driver);
            SetLeg7(-TurnAngle, RelaxedFemur, RelaxedTibia, driver);
            SetLeg8(TurnAngle, RelaxedFemur, RelaxedTibia, driver);
            Thread.Sleep(BreakTime);
            // leg 1 and 8 move
            SetLeg1(TurnAngle, LiftedFemur, LiftedTibia, driver);
            SetLeg8(TurnAngle, LiftedFemur, LiftedTibia, driver);
            Thread.Sleep(BreakTime);
            SetLeg1(-TurnAngle, LiftedFemur, LiftedTibia, driver);
            SetLeg8(-TurnAngle, LiftedFemur, LiftedTibia, driver);
            Thread.Sleep(BreakTime);
            SetLeg1(-TurnAngle, RelaxedFemur, RelaxedTibia, driver);
            SetLeg8(-TurnAngle, RelaxedFemur, RelaxedTibia, driver);
            Thread.Sleep(BreakTime);
            // leg 2 and 7 move
            SetLeg2(-TurnAngle, LiftedFemur, LiftedTibia, driver);
            SetLeg7(-TurnAngle, LiftedFemur, LiftedTibia, driver);
            Thread.Sleep(BreakTime);
            SetLeg2(TurnAngle, LiftedFemur, LiftedTibia, driver);
            SetLeg7(TurnAngle, LiftedFemur, LiftedTibia, driver);
            Thread.Sleep(BreakTime);
            SetLeg2(TurnAngle, RelaxedFemur, RelaxedTibia, driver);
            SetLeg7(TurnAngle, RelaxedFemur, RelaxedTibia, driver);
            Thread.Sleep(BreakTime);
        }

        public static void TurnRight(DynamixelDriver driver)
        {
            SetLeg1(-TurnAngle, RelaxedFemur, RelaxedTibia, driver);
            SetLeg2(TurnAngle, RelaxedFemur, RelaxedTibia, driver);
            SetLeg7(TurnAngle, RelaxedFemur, RelaxedTibia, driver);
            SetLeg8(-TurnAngle, RelaxedFemur, RelaxedTibia, driver);
            Thread.Sleep(BreakTime);
            // leg 1 and 8 move
            SetLeg1(-TurnAngle, LiftedFemur, RelaxedTibia, driver);
            SetLeg8(-TurnAngle, LiftedFemur, RelaxedTibia, driver);
            Thread.Sleep(50);
            SetLeg1(-TurnAngle, LiftedFemur, LiftedTibia, driver);
            SetLeg8(-TurnAngle, LiftedFemur, LiftedTibia, driver);
            Thread.Sleep(BreakTime);
            SetLeg1(TurnAngle, LiftedFemur, LiftedTibia, driver);
            SetLeg8(TurnAngle, LiftedFemur, LiftedTibia, driver);
            Thread.Sleep(BreakTime);
            SetLeg1(TurnAngle, RelaxedFemur, RelaxedTibia, driver);
            SetLeg8(TurnAngle, RelaxedFemur, RelaxedTibia, driver);
            Thread.Sleep(BreakTime);
            // leg 2 and 7 move
            SetLeg2(TurnAngle, LiftedFemur, LiftedTibia, driver);
            SetLeg7(TurnAngle, LiftedFemur, LiftedTibia, driver);
            Thread.Sleep(BreakTime);
            SetLeg2(-TurnAngle, LiftedFemur, LiftedTibia, driver);
            SetLeg7(-TurnAngle, LiftedFemur, LiftedTibia, driver);
            Thread.Sleep(BreakTime);
            SetLeg2(-TurnAngle, RelaxedFemur, RelaxedTibia, driver);
            SetLeg7(-TurnAngle, RelaxedFemur, RelaxedTibia, driver);
            Thread.Sleep(BreakTime);
        }

        public static void SetRobot(float coxa, float femur, float tibia, DynamixelDriver driver)
        {
            SetLeg1(coxa, femur, tibia, driver);
            SetLeg2(coxa, femur, tibia, driver);
            SetLeg7(coxa, femur, tibia, driver);
            SetLeg8(coxa, femur, tibia, driver);
        }

        public static void SetLeg1(float coxa, float femur, float tibia, DynamixelDriver driver)
        {
            int coxaUnits = DynamixelDriver.DegreesToUnits(150 - coxa);
            int femurUnits = DynamixelDriver.DegreesToUnits(150 + femur);
            int tibiaUnits = DynamixelDriver.DegreesToUnits(150 + tibia);
            // coxa
            ExceptionHelper.RepeatCatch(() => driver.SetGoalPosition(1, (ushort)coxaUnits));
            // femur
            ExceptionHelper.RepeatCatch(() => driver.SetGoalPosition(3, (ushort)femurUnits));
            // tibia
            ExceptionHelper.RepeatCatch(() => driver.SetGoalPosition(5, (ushort)tibiaUnits));
        }

        public static void SetLeg2(float coxa, float femur, float tibia, DynamixelDriver driver)
        {
            int coxaUnits = DynamixelDriver.DegreesToUnits(150 + coxa);
            int femurUnits = DynamixelDriver.DegreesToUnits(150 - femur);
            int tibiaUnits = DynamixelDriver.DegreesToUnits(150 - tibia);
            // coxa
            ExceptionHelper.RepeatCatch(() => driver.SetGoalPosition(2, (ushort)coxaUnits));
            // femur
            ExceptionHelper.RepeatCatch(() => driver.SetGoalPosition(4, (ushort)femurUnits), swallow: true);
            // tibia
            ExceptionHelper.RepeatCatch(() => driver.SetGoalPosition(6, (ushort)tibiaUnits), swallow: true);
        }

        public static void SetLeg7(float coxa, float femur, float tibia, DynamixelDriver driver)
        {
            int coxaUnits = DynamixelDriver.DegreesToUnits(150 + coxa);
            int femurUnits = DynamixelDriver.DegreesToUnits(150 - femur);
            int tibiaUnits = DynamixelDriver.DegreesToUnits(150 - tibia);
            // coxa
            ExceptionHelper.RepeatCatch(() => driver.SetGoalPosition(7, (ushort)coxaUnits));
            // femur
            ExceptionHelper.RepeatCatch(() => driver.SetGoalPosition(9, (ushort)femurUnits));
            // tibia
            ExceptionHelper.RepeatCatch(() => driver.SetGoalPosition(11, (ushort)tibiaUnits));
        }

        public static void SetLeg8(float coxa, float femur, float tibia, DynamixelDriver driver)
        {
            int coxaUnits = DynamixelDriver.DegreesToUnits(150 - coxa);
            int femurUnits = DynamixelDriver.DegreesToUnits(150 + femur);
            int tibiaUnits = DynamixelDriver.DegreesToUnits(150 + tibia);
            // coxa
            ExceptionHelper.RepeatCatch(() => driver.SetGoalPosition(8, (ushort)coxaUnits));
            // femur
            ExceptionHelper.RepeatCatch(() => driver.SetGoalPosition(10, (ushort)femurUnits));
            // tibia
            ExceptionHelper.RepeatCatch(() => driver.SetGoalPosition(12, (ushort)tibiaUnits));
        }
    }
}
