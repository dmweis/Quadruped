namespace DynamixelServo.Quadruped
{
    struct LegGoalPositions
    {
        public readonly float Coxa;
        public readonly float Femur;
        public readonly float Tibia;

        public LegGoalPositions(float coxa, float femur, float tibia)
        {
            Coxa = coxa;
            Femur = femur;
            Tibia = tibia;
        }

        public override string ToString()
        {
            return $"Coxa: {Coxa} Femur: {Femur} Tibia: {Tibia}";
        }
    }
}
