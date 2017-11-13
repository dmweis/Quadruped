namespace Quadruped.WebInterface.RobotController
{
    public class RobotConfig
    {
        public float FrontLegShift { get; set; } = 2;
        public float RearLegShift { get; set; } = 1;
        public float LiftHeight { get; set; } = 2;
        public int Speed { get; set; } = 30;
        public StepConfiguration StepConfig { get; set; } = StepConfiguration.OneStep;
    }

    public enum StepConfiguration
    {
        OneStep,
        TwoStep
    }
}
