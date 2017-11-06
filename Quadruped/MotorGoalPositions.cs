namespace Quadruped
{
    struct MotorGoalPositions
    {
        public readonly float Coxa;
        public readonly float Femur;
        public readonly float Tibia;

        public MotorGoalPositions(float coxa, float femur, float tibia)
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
