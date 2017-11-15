using System.Numerics;

namespace Quadruped
{
    public class LegPositions
    {
        public Vector3 LeftFront { get; }
        public Vector3 RightFront { get; }
        public Vector3 LeftRear { get; }
        public Vector3 RightRear { get; }

        public LegPositions(Vector3 leftFront, Vector3 rightFront, Vector3 leftRear, Vector3 rightRear)
        {
            LeftFront = leftFront;
            RightFront = rightFront;
            LeftRear = leftRear;
            RightRear = rightRear;
        }

        public LegPositions()
        {
            LeftFront = Vector3.Zero;
            RightFront = Vector3.Zero;
            LeftRear = Vector3.Zero;
            RightRear = Vector3.Zero;
        }

        public LegPositions Transform(Vector3 transformVector, LegFlags legs = LegFlags.All)
        {
            Vector3 newLeftFront = LeftFront;
            Vector3 newRightFront = RightFront;
            Vector3 newLeftRear = LeftRear;
            Vector3 newRightRear = RightRear;
            if ((legs & LegFlags.LeftFront) != 0)
            {
                newLeftFront = LeftFront + transformVector;
            }
            if ((legs & LegFlags.RightFront) != 0)
            {
                newRightFront = RightFront + transformVector;
            }
            if ((legs & LegFlags.LeftRear) != 0)
            {
                newLeftRear = LeftRear + transformVector;
            }
            if ((legs & LegFlags.RightRear) != 0)
            {
                newRightRear = RightRear + transformVector;
            }
            return new LegPositions(newLeftFront, newRightFront, newLeftRear, newRightRear);
        }

        public LegPositions Rotate(Angle angle, LegFlags legs = LegFlags.All)
        {
            Vector3 newLeftFront = LeftFront;
            Vector3 newRightFront = RightFront;
            Vector3 newLeftRear = LeftRear;
            Vector3 newRightRear = RightRear;
            if ((legs & LegFlags.LeftFront) != 0)
            {
                newLeftFront = new Vector3(LeftFront.ToDirectionVector2().Rotate(angle), LeftFront.Z);
            }
            if ((legs & LegFlags.RightFront) != 0)
            {
                newRightFront = new Vector3(RightFront.ToDirectionVector2().Rotate(angle), RightFront.Z);
            }
            if ((legs & LegFlags.LeftRear) != 0)
            {
                newLeftRear = new Vector3(LeftRear.ToDirectionVector2().Rotate(angle), LeftRear.Z);
            }
            if ((legs & LegFlags.RightRear) != 0)
            {
                newRightRear = new Vector3(RightRear.ToDirectionVector2().Rotate(angle), RightRear.Z);
            }
            return new LegPositions(newLeftFront, newRightFront, newLeftRear, newRightRear);
        }

        /// <summary>
        /// Rotate the conter of the robot by degrees
        /// </summary>
        /// <param name="rotation"></param>
        /// <param name="legs"></param>
        public LegPositions RotateCenter(Rotation rotation, LegFlags legs = LegFlags.All)
        {
            return RotateCenter(Quaternion.CreateFromYawPitchRoll(rotation.Pitch.DegreeToRad(), rotation.Roll.DegreeToRad(), rotation.Yaw.DegreeToRad()), legs);
        }

        private LegPositions RotateCenter(Quaternion rotation, LegFlags legs = LegFlags.All)
        {
            Vector3 newLeftFront = LeftFront;
            Vector3 newRightFront = RightFront;
            Vector3 newLeftRear = LeftRear;
            Vector3 newRightRear = RightRear;
            if ((legs & LegFlags.LeftFront) != 0)
            {
                newLeftFront = Vector3.Transform(LeftFront, rotation);
            }
            if ((legs & LegFlags.RightFront) != 0)
            {
                newRightFront = Vector3.Transform(RightFront, rotation);
            }
            if ((legs & LegFlags.LeftRear) != 0)
            {
                newLeftRear = Vector3.Transform(LeftRear, rotation);
            }
            if ((legs & LegFlags.RightRear) != 0)
            {
                newRightRear = Vector3.Transform(RightRear, rotation);
            }
            return new LegPositions(newLeftFront, newRightFront, newLeftRear, newRightRear);
        }

        public LegPositions MoveTowards(LegPositions target, float distance)
        {
            var newLeftFront = LeftFront.MoveTowards(target.LeftFront, distance);
            var newRightFront = RightFront.MoveTowards(target.RightFront, distance);
            var newLeftRear = LeftRear.MoveTowards(target.LeftRear, distance);
            var newRightRear = RightRear.MoveTowards(target.RightRear, distance);
            return new LegPositions(newLeftFront, newRightFront, newLeftRear, newRightRear);
        }

        public bool MoveFinished(LegPositions other)
        {
            return LeftFront == other.LeftFront &&
                   RightFront == other.RightFront &&
                   LeftRear == other.LeftRear &&
                   RightRear == other.RightRear;
        }

        public override string ToString()
        {
            return $"RightFront {RightFront} RightRear {RightRear} LeftFront {LeftFront} LeftRear {LeftRear}";
        }
    }
}
