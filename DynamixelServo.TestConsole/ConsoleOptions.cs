using System;

namespace DynamixelServo.TestConsole
{
    [Flags]
    enum ConsoleOptions
    {
        GaitEngine = 1,
        MeasureLimits = 2,
        TrackerListener = 4,
        SelectOption = 8,
        DriverTest = 16,
        BasicGaitEngine = 32,
        BasicGaitEngineXbox = 64
    }
}
