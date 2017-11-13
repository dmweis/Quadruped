namespace Quadruped.MotionPlanning
{
    public enum StartingStanceOptions
    {
        // Enum is numbered to make sure Relaxed is the default value. This is to ensue that if it's not specified it will be Relaxed
        Relaxed = 0,
        Previous = 1
    }
}
