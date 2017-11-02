using System.Numerics;
using DynamixelServo.Driver;

namespace DynamixelServo.Quadruped.WebInterface.VideoStreaming
{
    public class CameraController : ICameraController
    {
        private readonly DynamixelDriver _driver;
        private const byte HorizontalMotorIndex = 13;
        private const byte VerticalMotorIndex = 14;

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
                _driver.SetGoalPositionInDegrees(HorizontalMotorIndex, 150);
                _driver.SetGoalPositionInDegrees(VerticalMotorIndex, 60);
            }
        }

        public void StartMove(Vector2 direction)
        {
            const float deadzone = 0.5f;
            lock (_driver.SyncLock)
            {
                _driver.SetMovingSpeed(HorizontalMotorIndex, 30);
                _driver.SetMovingSpeed(VerticalMotorIndex, 20);
                if (direction.X > deadzone)
                {
                    _driver.SetGoalPositionInDegrees(HorizontalMotorIndex, 0);
                }
                else if (direction.X < -deadzone)
                {
                    _driver.SetGoalPositionInDegrees(HorizontalMotorIndex, 300);
                }

                if (direction.Y > deadzone)
                {
                    _driver.SetGoalPositionInDegrees(VerticalMotorIndex, 270);
                }
                else if (direction.Y < -deadzone)
                {
                    _driver.SetGoalPositionInDegrees(VerticalMotorIndex, 30);
                }
            }
        }

        public void StopMove()
        {
            lock (_driver.SyncLock)
            {
                var currentPos = _driver.GetPresentPosition(HorizontalMotorIndex);
                _driver.SetGoalPosition(HorizontalMotorIndex, currentPos);
                currentPos = _driver.GetPresentPosition(VerticalMotorIndex);
                _driver.SetGoalPosition(VerticalMotorIndex, currentPos);
            }
        }
    }
}
