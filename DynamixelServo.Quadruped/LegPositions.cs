using System.Numerics;

namespace DynamixelServo.Quadruped
{
    public class LegPositions
    {
        public Vector3 LeftFront { get; set; }
        public Vector3 RightFront { get; set; }
        public Vector3 LeftRear { get; set; }
        public Vector3 RightRear { get; set; }

        public LegPositions(QuadrupedIkDriver driver)
        {
            LeftFront = driver.GetLeftFrontLegGoal();
            RightFront = driver.GetRightFrontLegGoal();
            LeftRear = driver.GetLeftRearLegGoal();
            RightRear = driver.GetRightRearLegGoal();
        }

        public LegPositions(LegPositions legPositions)
        {
            LeftFront = legPositions.LeftFront;
            RightFront = legPositions.RightFront;
            LeftRear = legPositions.LeftRear;
            RightRear = legPositions.RightRear;
        }

        public LegPositions(Vector3 leftFront, Vector3 rightFront, Vector3 leftRear, Vector3 rightRear)
        {
            LeftFront = leftFront;
            RightFront = rightFront;
            LeftRear = leftRear;
            RightRear = rightRear;
        }

        public LegPositions()
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

        public void Rotate(Angle angle, LegFlags legs = LegFlags.All)
        {
            if ((legs & LegFlags.LeftFront) != 0)
            {
                LeftFront  = new Vector3(LeftFront.ToDirectionVector2().Rotate(angle), LeftFront.Z);
            }
            if ((legs & LegFlags.RightFront) != 0)
            {
                RightFront = new Vector3(RightFront.ToDirectionVector2().Rotate(angle), RightFront.Z);
            }
            if ((legs & LegFlags.LeftRear) != 0)
            {
                LeftRear = new Vector3(LeftRear.ToDirectionVector2().Rotate(angle), LeftRear.Z);
            }
            if ((legs & LegFlags.RightRear) != 0)
            {
                RightRear = new Vector3(RightRear.ToDirectionVector2().Rotate(angle), RightRear.Z);
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

        public void MoveTowards(LegPositions target, float distance)
        {
            LeftFront = LeftFront.MoveTowards(target.LeftFront, distance);
            RightFront = RightFront.MoveTowards(target.RightFront, distance);
            LeftRear = LeftRear.MoveTowards(target.LeftRear, distance);
            RightRear = RightRear.MoveTowards(target.RightRear, distance);
        }

        public LegPositions GetShiftedTowards(LegPositions target, float distance)
        {
            var newLegPosition = Copy();
            newLegPosition.LeftFront = LeftFront.MoveTowards(target.LeftFront, distance);
            newLegPosition.RightFront = RightFront.MoveTowards(target.RightFront, distance);
            newLegPosition.LeftRear = LeftRear.MoveTowards(target.LeftRear, distance);
            newLegPosition.RightRear = RightRear.MoveTowards(target.RightRear, distance);
            return newLegPosition;
        }

        public bool MoveFinished(LegPositions other)
        {
            return LeftFront == other.LeftFront &&
                   RightFront == other.RightFront &&
                   LeftRear == other.LeftRear &&
                   RightRear == other.RightRear;
        }

        public bool CloserThan(LegPositions target, float distance)
        {
            float distanceSquared = distance.Square();
            if (RightFront.DistanceSquared(target.RightFront) > distanceSquared)
            {
                return false;
            }
            if (RightRear.DistanceSquared(target.RightRear) > distanceSquared)
            {
                return false;
            }
            if (LeftFront.DistanceSquared(target.LeftFront) > distanceSquared)
            {
                return false;
            }
            if (LeftRear.DistanceSquared(target.LeftRear) > distanceSquared)
            {
                return false;
            }
            return true;
        }

        public LegPositions Copy()
        {
            return new LegPositions(this);
        }

        public override string ToString()
        {
            return $"RightFront {RightFront} RightRear {RightRear} LeftFront {LeftFront} LeftRear {LeftRear}";
        }
    }
}
