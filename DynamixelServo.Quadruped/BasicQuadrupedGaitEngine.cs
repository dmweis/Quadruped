using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DynamixelServo.Quadruped
{
    public class BasicQuadrupedGaitEngine : QuadrupedGaitEngine
    {
        private const int Speed = 6;
        private float NextStepLength => Speed * 0.001f * TimeSincelastTick;

        private readonly Queue<MotorPositions> _moves = new Queue<MotorPositions>();

        private MotorPositions _nextMove;

        private const float LegHeight = -11f;
        private const int LegDistance = 15;

        private readonly MotorPositions _lastWrittenPosition;

        public BasicQuadrupedGaitEngine(QuadrupedIkDriver driver) : base(driver)
        {
            EnqueuePositions();
            Driver.Setup();
            Driver.StandUpfromGround();
            _lastWrittenPosition = new MotorPositions(driver);
            _nextMove = _moves.Dequeue();
            StartEngine();
        }

        protected override void EngineSpin()
        {
            _lastWrittenPosition.MoveTowards(_nextMove, NextStepLength);
            _lastWrittenPosition.MoveRobot(Driver);
            if (_lastWrittenPosition.MoveFinished(_nextMove))
            {
                if (_moves.Count > 0)
                {
                    _nextMove = _moves.Dequeue();
                }
            }
        }

        public void EnqueuePositions()
        {
            _moves.Enqueue(new MotorPositions
            {
                LeftFront = new Vector3(-LegDistance, LegDistance + 5, LegHeight),
                RightFront = new Vector3(LegDistance, LegDistance, LegHeight),
                LeftRear = new Vector3(-LegDistance + 2.5f, -LegDistance + 2.5f, LegHeight),
                RightRear = new Vector3(LegDistance, -LegDistance + 5, LegHeight)
            });
            _moves.Enqueue(new MotorPositions
            {
                LeftFront = new Vector3(-LegDistance, LegDistance + 5, LegHeight),
                RightFront = new Vector3(LegDistance, LegDistance, LegHeight + 3),
                LeftRear = new Vector3(-LegDistance + 2.5f, -LegDistance + 2.5f, LegHeight),
                RightRear = new Vector3(LegDistance, -LegDistance + 5, LegHeight)
            });
            _moves.Enqueue(new MotorPositions
            {
                LeftFront = new Vector3(-LegDistance, LegDistance + 5, LegHeight),
                RightFront = new Vector3(LegDistance + 5, LegDistance + 5, LegHeight + 3),
                LeftRear = new Vector3(-LegDistance + 2.5f, -LegDistance + 2.5f, LegHeight),
                RightRear = new Vector3(LegDistance, -LegDistance + 5, LegHeight)
            });
            _moves.Enqueue(new MotorPositions
            {
                LeftFront = new Vector3(-LegDistance, LegDistance + 5, LegHeight),
                RightFront = new Vector3(LegDistance + 5, LegDistance + 5, LegHeight),
                LeftRear = new Vector3(-LegDistance + 2.5f, -LegDistance + 2.5f, LegHeight),
                RightRear = new Vector3(LegDistance, -LegDistance + 5, LegHeight)
            });
            _moves.Enqueue(new MotorPositions
            {
                LeftFront = new Vector3(-LegDistance, LegDistance + 5, LegHeight),
                RightFront = new Vector3(LegDistance + 5, LegDistance + 5, LegHeight + 3),
                LeftRear = new Vector3(-LegDistance + 2.5f, -LegDistance + 2.5f, LegHeight),
                RightRear = new Vector3(LegDistance, -LegDistance + 5, LegHeight)
            });
            _moves.Enqueue(new MotorPositions
            {
                LeftFront = new Vector3(-LegDistance, LegDistance + 5, LegHeight),
                RightFront = new Vector3(LegDistance, LegDistance, LegHeight + 3),
                LeftRear = new Vector3(-LegDistance + 2.5f, -LegDistance + 2.5f, LegHeight),
                RightRear = new Vector3(LegDistance, -LegDistance + 5, LegHeight)
            });
            _moves.Enqueue(new MotorPositions
            {
                LeftFront = new Vector3(-LegDistance, LegDistance + 5, LegHeight),
                RightFront = new Vector3(LegDistance, LegDistance, LegHeight),
                LeftRear = new Vector3(-LegDistance + 2.5f, -LegDistance + 2.5f, LegHeight),
                RightRear = new Vector3(LegDistance, -LegDistance + 5, LegHeight)
            });
        }
    }

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
    }
}
