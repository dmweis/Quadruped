using System;

namespace DynamixelServo.TestConsole
{
    [Flags]
    enum ConsoleOptions
    {
        GaitEngine = 1,
        OldIkEngine = 2,
        MeasureLimits = 4,
        TrackerListener = 8,
        SelectOption = 16,
        DriverTest = 32
    }
}
