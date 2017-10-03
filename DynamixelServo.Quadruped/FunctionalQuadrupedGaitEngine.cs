using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading;

namespace DynamixelServo.Quadruped
{
    public class FunctionalQuadrupedGaitEngine : QuadrupedGaitEngine
    {

        private const float LegHeight = -11f;
        private const int LegDistance = 15;

        private MotorPositions RelaxedStance
        {
            get
            {
                return new MotorPositions
                {
                    LeftFront = new Vector3(-LegDistance, LegDistance, LegHeight),
                    RightFront = new Vector3(LegDistance, LegDistance, LegHeight),
                    LeftRear = new Vector3(-LegDistance, -LegDistance, LegHeight),
                    RightRear = new Vector3(LegDistance, -LegDistance, LegHeight)
                }.Copy();
            }
        }


        public FunctionalQuadrupedGaitEngine(QuadrupedIkDriver driver) : base(driver)
        {
            Driver.Setup();
            var currentPosition = new MotorPositions(driver);
            float average = (currentPosition.LeftFront.Z +
                             currentPosition.RightFront.Z +
                             currentPosition.LeftRear.Z +
                             currentPosition.RightRear.Z) / 4;
            if (average > -9)
            {
                new MotorPositions
                {
                    LeftFront = new Vector3(-LegDistance + 6, LegDistance + 6, 0),
                    RightFront = new Vector3(LegDistance, LegDistance, 0),
                    LeftRear = new Vector3(-LegDistance, -LegDistance, 0),
                    RightRear = new Vector3(LegDistance + 6, -LegDistance + 6, 0)
                }.MoveRobot(Driver);
                Thread.Sleep(1000);
            }
            new MotorPositions
            {
                LeftFront = new Vector3(-LegDistance + 6, LegDistance + 6, LegHeight),
                RightFront = new Vector3(LegDistance, LegDistance, LegHeight),
                LeftRear = new Vector3(-LegDistance, -LegDistance, LegHeight),
                RightRear = new Vector3(LegDistance + 6, -LegDistance + 6, LegHeight)
            }.MoveRobot(Driver);
            StartEngine();
        }

        protected override void EngineSpin()
        {
            double angle = TimeSinceStart / 1000.0 * 90.0;
            double sin = Math.Sin(angle * Math.PI / 180);
            double cos = Math.Cos(angle * Math.PI / 180);
            try
            {
                //Driver.MoveAbsoluteCenterMass(new Vector3((float)(6 * sin), -(float)(6 * cos), 0), LegDistance, LegHeight );
                Driver.MoveRightFrontLeg(new Vector3((float)(cos * 4.0) + LegDistance, -(float)(cos * 4.0) + LegDistance, (float)Math.Abs(sin * 4) + LegHeight));
            }
            catch (IOException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                Console.ResetColor();
                throw;
            }
        }
    }
}
