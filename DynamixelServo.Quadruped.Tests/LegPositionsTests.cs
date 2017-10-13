using System.Numerics;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DynamixelServo.Quadruped.Tests
{
    [TestClass()]
    public class LegPositionsTests
    {
        private const float FloatComparasionPrecision = 0.001f;

        [TestMethod()]
        public void RotatingLegPositionShouldOnlyMoveCorrectLegs()
        {
            // Arrange
            LegPositions positions = new LegPositions(Vector3.UnitX, Vector3.UnitX, Vector3.UnitX, Vector3.UnitX);
            // Act
            positions.Rotate(new Angle(90), LegFlags.RfLrCross);
            // Assert
            positions.RightFront.X.Should().BeApproximately(0, FloatComparasionPrecision);
            positions.RightFront.Y.Should().BeApproximately(1, FloatComparasionPrecision);

            positions.LeftRear.X.Should().BeApproximately(0, FloatComparasionPrecision);
            positions.LeftRear.Y.Should().BeApproximately(1, FloatComparasionPrecision);

            positions.RightRear.X.Should().BeApproximately(1, FloatComparasionPrecision);
            positions.RightRear.Y.Should().BeApproximately(0, FloatComparasionPrecision);

            positions.LeftFront.X.Should().BeApproximately(1, FloatComparasionPrecision);
            positions.LeftFront.Y.Should().BeApproximately(0, FloatComparasionPrecision);
        }

        [TestMethod()]
        public void RotatingLegPositionBy0ShouldNotChangeIt()
        {
            // Arrange
            LegPositions positions = new LegPositions(Vector3.UnitX, Vector3.UnitX, Vector3.UnitX, Vector3.UnitX);
            // Act
            positions.Rotate(new Angle(0), LegFlags.RfLrCross);
            // Assert
            positions.RightFront.ShouldBeEquivalentTo(Vector3.UnitX);

            positions.LeftRear.ShouldBeEquivalentTo(Vector3.UnitX);

            positions.RightRear.ShouldBeEquivalentTo(Vector3.UnitX);

            positions.LeftFront.ShouldBeEquivalentTo(Vector3.UnitX);
        }
    }
}