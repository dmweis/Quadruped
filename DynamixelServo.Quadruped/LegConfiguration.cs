namespace DynamixelServo.Quadruped
{
    public class LegConfiguration
    {
        public byte CoxaId { get; }
        public byte FemurId { get; }
        public byte TibiaId { get; }
        public float AngleOffset { get; }
        public Vector3 CoxaPosition { get; }

        public float FemurCorrection { get; }
        public float TibiaCorrection { get; }

        public LegConfiguration(byte coxaId, byte femurId, byte tibiaId, float angleOffset, Vector3 coxaPosition, float femurCorrection, float tibiaCorrection)
        {
            CoxaId = coxaId;
            FemurId = femurId;
            TibiaId = tibiaId;
            AngleOffset = angleOffset;
            CoxaPosition = coxaPosition;
            FemurCorrection = femurCorrection;
            TibiaCorrection = tibiaCorrection;
        }
    }
}
