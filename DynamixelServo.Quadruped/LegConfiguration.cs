namespace DynamixelServo.Quadruped
{
    public class LegConfiguration
    {
        public byte CoxaId { get; set; }
        public byte FemurId { get; set; }
        public byte TibiaId { get; set; }
        public float AngleOffset { get; set; }
        public Vector3 CoxaPosition { get; set; }

        public float FemurCorrection { get; set; }
        public float TibiaCorrection { get; set; }

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
