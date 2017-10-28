using System.Numerics;
using DynamixelServo.Driver;

namespace DynamixelServo.Quadruped.WebInterface.VideoStreaming
{
    public class CameraController : ICameraController
    {
        private readonly DynamixelDriver _driver;
        private const byte HorizontalMotorIndex = 13;
        private const byte VerticalMotorIndex = 13;

        public CameraController(DynamixelDriver driver)
        {
            _driver = driver;
            CenterView();
        }

        public void CenterView()
        {
            _driver.SetMovingSpeed(HorizontalMotorIndex, 0);
            _driver.SetMovingSpeed(VerticalMotorIndex, 0);
            _driver.SetGoalPositionInDegrees(HorizontalMotorIndex, 150);
            _driver.SetGoalPositionInDegrees(VerticalMotorIndex, 240);
        }

        public void StartMove(Vector2 direction)
        {
            _driver.SetMovingSpeed(HorizontalMotorIndex, 20);
            _driver.SetMovingSpeed(VerticalMotorIndex, 20);
            if (direction.X > 0)
            {
                _driver.SetGoalPositionInDegrees(HorizontalMotorIndex, 0);
            }
            else if (direction.X < 0)
            {
                _driver.SetGoalPositionInDegrees(HorizontalMotorIndex, 300);
            }

            if (direction.Y > 0)
            {
                _driver.SetGoalPositionInDegrees(VerticalMotorIndex, 240);
            }
            else if (direction.Y < 0)
            {
                _driver.SetGoalPositionInDegrees(VerticalMotorIndex, 60);
            }
        }

        public void StopMove()
        {
            var currentPos = _driver.GetGoalPosition(HorizontalMotorIndex);
            _driver.SetGoalPosition(HorizontalMotorIndex, currentPos);
            currentPos = _driver.GetGoalPosition(VerticalMotorIndex);
            _driver.SetGoalPosition(VerticalMotorIndex, currentPos);
        }
    }
}
