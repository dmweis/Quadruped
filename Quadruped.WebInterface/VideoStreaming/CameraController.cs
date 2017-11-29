using System;
using System.Numerics;
using Quadruped.Driver;

namespace Quadruped.WebInterface.VideoStreaming
{
    public class CameraController : ICameraController
    {
        private readonly DynamixelDriver _driver;
        private const byte HorizontalMotorIndex = 13;
        private const byte VerticalMotorIndex = 14;

        private const int HorizontalCenter = 150;
        private const int VerticalCenter = 60;

        private bool _centering;

        public CameraController(DynamixelDriver driver)
        {
            _driver = driver;
            CenterView();
        }

        public void CenterView()
        {
            lock (_driver.SyncLock)
            {
                _driver.SetMovingSpeed(HorizontalMotorIndex, 300);
                _driver.SetMovingSpeed(VerticalMotorIndex, 300);
                _driver.SetGoalPositionInDegrees(HorizontalMotorIndex, HorizontalCenter);
                _driver.SetGoalPositionInDegrees(VerticalMotorIndex, VerticalCenter);
            }
            _centering = true;
        }

        private bool CenteringDone()
        {
            if (_centering)
            {
                lock (_driver.SyncLock)
                {
                    var centered = AreClose(_driver.GetPresentPositionInDegrees(HorizontalMotorIndex), HorizontalCenter) &&
                    AreClose(_driver.GetPresentPositionInDegrees(VerticalMotorIndex), VerticalCenter);
                    _centering = !centered;
                    return centered;
                }
            }
            return true;
        }

        private static bool AreClose(float a, float b, float maxDifference = 2f)
        {
            return Math.Abs(a - b) < maxDifference;
        }

        public void StartMove(Vector2 direction)
        {
            if (!CenteringDone())
            {
                return;
            }
            const float deadzone = 0.5f;
            lock (_driver.SyncLock)
            {
                _driver.SetMovingSpeed(HorizontalMotorIndex, 30);
                _driver.SetMovingSpeed(VerticalMotorIndex, 20);
                if (-direction.Y > deadzone)
                {
                    _driver.SetGoalPositionInDegrees(HorizontalMotorIndex, 0);
                }
                else if (-direction.Y < -deadzone)
                {
                    _driver.SetGoalPositionInDegrees(HorizontalMotorIndex, 300);
                }
                else
                {
                    var currentPos = _driver.GetPresentPosition(HorizontalMotorIndex);
                    _driver.SetGoalPosition(HorizontalMotorIndex, currentPos);
                }

                if (direction.X > deadzone)
                {
                    _driver.SetGoalPositionInDegrees(VerticalMotorIndex, 270);
                }
                else if (direction.X < -deadzone)
                {
                    _driver.SetGoalPositionInDegrees(VerticalMotorIndex, 30);
                }
                else
                {
                    var currentPos = _driver.GetPresentPosition(VerticalMotorIndex);
                    _driver.SetGoalPosition(VerticalMotorIndex, currentPos);
                }
            }
        }
    }
}
