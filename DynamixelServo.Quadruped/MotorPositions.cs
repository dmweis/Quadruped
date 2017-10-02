using System;
using System.Numerics;

namespace DynamixelServo.Quadruped
{
    class MotorPositions
    {
        public Vector3 LeftFront { get; set; }
        public Vector3 RightFront { get; set; }
        public Vector3 LeftRear { get; set; }
        public Vector3 RightRear { get; set; }

        public MotorPositions(QuadrupedIkDriver driver)
        {
            LeftFront = driver.GetLeftFrontLegGoal();
            RightFront = driver.GetRightFrontLegGoal();
            LeftRear = driver.GetLeftRearLegGoal();
            RightRear = driver.GetRightRearLegGoal();
        }

        public MotorPositions(MotorPositions motorPositions)
        {
            LeftFront = motorPositions.LeftFront;
            RightFront = motorPositions.RightFront;
            LeftRear = motorPositions.LeftRear;
            RightRear = motorPositions.RightRear;
        }

        public MotorPositions(Vector3 leftFront, Vector3 rightFront, Vector3 leftRear, Vector3 rightRear)
        {
            LeftFront = leftFront;
            RightFront = rightFront;
            LeftRear = leftRear;
            RightRear = rightRear;
        }

        public MotorPositions()
        {
            LeftFront = new Vector3();
            RightFront = new Vector3();
            LeftRear = new Vector3();
            RightRear = new Vector3();
        }

        public void Transform(Vector3 transformVector, LegFlags legs = LegFlags.All)
        {
            if ((legs & LegFlags.LeftFront) != 0)
            {
                LeftFront += transformVector;
            }
            if ((legs & LegFlags.RightFront) != 0)
            {
                RightFront += transformVector;
            }
            if ((legs & LegFlags.LeftRear) != 0)
            {
                LeftRear += transformVector;
            }
            if ((legs & LegFlags.RightRear) != 0)
            {
                RightRear += transformVector;
            }
        }

        public void SetLegs(Vector3 newVector, LegFlags legs = LegFlags.All)
        {
            if ((legs & LegFlags.LeftFront) != 0)
            {
                LeftFront = newVector;
            }
            if ((legs & LegFlags.RightFront) != 0)
            {
                RightFront = newVector;
            }
            if ((legs & LegFlags.LeftRear) != 0)
            {
                LeftRear = newVector;
            }
            if ((legs & LegFlags.RightRear) != 0)
            {
                RightRear = newVector;
            }
        }

        public void MoveTowards(MotorPositions target, float distance)
        {
            LeftFront = LeftFront.MoveTowards(target.LeftFront, distance);
            RightFront = RightFront.MoveTowards(target.RightFront, distance);
            LeftRear = LeftRear.MoveTowards(target.LeftRear, distance);
            RightRear = RightRear.MoveTowards(target.RightRear, distance);
        }

        public void MoveRobot(QuadrupedIkDriver driver)
        {
            driver.MoveLeftFrontLeg(LeftFront);
            driver.MoveRightFrontLeg(RightFront);
            driver.MoveLeftRearLeg(LeftRear);
            driver.MoveRightRearLeg(RightRear);
        }

        public bool MoveFinished(MotorPositions other)
        {
            return LeftFront == other.LeftFront &&
                   RightFront == other.RightFront &&
                   LeftRear == other.LeftRear &&
                   RightRear == other.RightRear;
        }

        public MotorPositions Copy()
        {
            return new MotorPositions(this);
        }
    }

    [Flags]
    enum LegFlags
    {
        LeftFront = 1,
        RightFront = 2,
        LeftRear = 4,
        RightRear = 8,
        All = LeftFront | RightFront | LeftRear | RightRear,
        Front = LeftFront | RightFront,
        Rear = LeftRear | RightRear,
        Right = RightFront | RightRear,
        Left = LeftFront | LeftRear
    }
}
