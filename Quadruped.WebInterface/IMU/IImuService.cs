using System;

namespace Quadruped.WebInterface.IMU
{
    public interface IImuService
    {
        event EventHandler<ImuData> NewImuData;
    }
}
