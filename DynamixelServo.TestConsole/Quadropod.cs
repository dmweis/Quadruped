using System.Threading;
using DynamixelServo.Driver;

namespace DynamixelServo.TestConsole
{
    class Quadropod
    {
        private const int TurnAngle = 40;

        public static void SetupAndStance(DynamixelDriver driver)
        {
            foreach (var servo in driver.Search(1, 20))
            {
                driver.SetComplianceSlope(servo, ComplianceSlope.S128);
                driver.SetMovingSpeed(servo, 500);
            }
            SetRobot(0, 45, 45, driver);
        }

        public static void Stance(DynamixelDriver driver)
        {
            SetRobot(0, 45, 45, driver);
        }

        public static void TurnLeft(DynamixelDriver driver)
        {
            const int breakTime = 75;
            SetLeg1(TurnAngle, 45, 45, driver);
            SetLeg2(-TurnAngle, 45, 45, driver);
            SetLeg7(-TurnAngle, 45, 45, driver);
            SetLeg8(TurnAngle, 45, 45, driver);
            Thread.Sleep(breakTime);
            // leg 1 move
            SetLeg1(TurnAngle, 0, 60, driver);
            Thread.Sleep(breakTime);
            SetLeg1(-TurnAngle, 0, 60, driver);
            Thread.Sleep(breakTime);
            SetLeg1(-TurnAngle, 45, 45, driver);
            Thread.Sleep(breakTime);
            // leg 8 move
            SetLeg8(TurnAngle, 0, 60, driver);
            Thread.Sleep(breakTime);
            SetLeg8(-TurnAngle, 0, 60, driver);
            Thread.Sleep(breakTime);
            SetLeg8(-TurnAngle, 45, 45, driver);
            Thread.Sleep(breakTime);
            // leg 2 move
            SetLeg2(-TurnAngle, 0, 60, driver);
            Thread.Sleep(breakTime);
            SetLeg2(TurnAngle, 0, 60, driver);
            Thread.Sleep(breakTime);
            SetLeg2(TurnAngle, 45, 45, driver);
            Thread.Sleep(breakTime);
            // leg 7 move
            SetLeg7(-TurnAngle, 0, 60, driver);
            Thread.Sleep(breakTime);
            SetLeg7(TurnAngle, 0, 60, driver);
            Thread.Sleep(breakTime);
            SetLeg7(TurnAngle, 45, 45, driver);
        }

        public static void TurnRight(DynamixelDriver driver)
        {
            const int breakTime = 75;
            SetLeg1(-TurnAngle, 45, 45, driver);
            SetLeg2(TurnAngle, 45, 45, driver);
            SetLeg7(TurnAngle, 45, 45, driver);
            SetLeg8(-TurnAngle, 45, 45, driver);
            Thread.Sleep(breakTime);
            // leg 1 move
            SetLeg1(-TurnAngle, 0, 60, driver);
            Thread.Sleep(breakTime);
            SetLeg1(TurnAngle, 0, 60, driver);
            Thread.Sleep(breakTime);
            SetLeg1(TurnAngle, 45, 45, driver);
            Thread.Sleep(breakTime);
            // leg 8 move
            SetLeg8(-TurnAngle, 0, 60, driver);
            Thread.Sleep(breakTime);
            SetLeg8(TurnAngle, 0, 60, driver);
            Thread.Sleep(breakTime);
            SetLeg8(TurnAngle, 45, 45, driver);
            Thread.Sleep(breakTime);
            // leg 2 move
            SetLeg2(TurnAngle, 0, 60, driver);
            Thread.Sleep(breakTime);
            SetLeg2(-TurnAngle, 0, 60, driver);
            Thread.Sleep(breakTime);
            SetLeg2(-TurnAngle, 45, 45, driver);
            Thread.Sleep(breakTime);
            // leg 7 move
            SetLeg7(TurnAngle, 0, 60, driver);
            Thread.Sleep(breakTime);
            SetLeg7(-TurnAngle, 0, 60, driver);
            Thread.Sleep(breakTime);
            SetLeg7(-TurnAngle, 45, 45, driver);
        }

        public static void SetRobot(float coxa, float femur, float tibia, DynamixelDriver driver)
        {
            int coxaUnits = DynamixelDriver.DegreesToUnits(coxa);
            int femurUnits = DynamixelDriver.DegreesToUnits(femur);
            int tibiaUnits = DynamixelDriver.DegreesToUnits(tibia);
            // coxa
            ExceptionHelper.RepeatCatch(() => driver.SetGoalPosition(1, (ushort)(512 - coxaUnits)));
            ExceptionHelper.RepeatCatch(() => driver.SetGoalPosition(2, (ushort)(512 + coxaUnits)));
            ExceptionHelper.RepeatCatch(() => driver.SetGoalPosition(7, (ushort)(512 + coxaUnits)));
            ExceptionHelper.RepeatCatch(() => driver.SetGoalPosition(8, (ushort)(512 - coxaUnits)));
            // femur
            ExceptionHelper.RepeatCatch(() => driver.SetGoalPosition(3, (ushort)(512 + femurUnits)));
            ExceptionHelper.RepeatCatch(() => driver.SetGoalPosition(4, (ushort)(512 - femurUnits)), swallow: true);
            ExceptionHelper.RepeatCatch(() => driver.SetGoalPosition(9, (ushort)(512 - femurUnits)));
            ExceptionHelper.RepeatCatch(() => driver.SetGoalPosition(10, (ushort)(512 + femurUnits)));
            // tibia
            ExceptionHelper.RepeatCatch(() => driver.SetGoalPosition(5, (ushort)(512 + tibiaUnits)));
            ExceptionHelper.RepeatCatch(() => driver.SetGoalPosition(6, (ushort)(512 - tibiaUnits)), swallow: true);
            ExceptionHelper.RepeatCatch(() => driver.SetGoalPosition(11, (ushort)(512 - tibiaUnits)));
            ExceptionHelper.RepeatCatch(() => driver.SetGoalPosition(12, (ushort)(512 + tibiaUnits)));
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
