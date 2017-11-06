using System.Numerics;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Quadruped.Tests
{
    [TestClass()]
    public class VectorExtensionsTests
    {

        private const float FloatComparasionPrecision = 0.001f;

        [TestMethod()]
        public void Vector2RotatedBy90ShouldRotate()
        {
            // Arange
            Vector2 one = new Vector2(1, 0);
            Angle degrees = new Angle(90f);
            // Act
            Vector2 turned = one.Rotate(degrees);
            // Assert
            turned.X.Should().BeApproximately(0, FloatComparasionPrecision);
            turned.Y.Should().BeApproximately(1, FloatComparasionPrecision);
        }

        [TestMethod()]
        public void Vector2RotatedByMinus90ShouldRotate()
        {
            // Arrange
            Vector2 one = new Vector2(1, 0);
            Angle degrees = new Angle(-90f);
            // Act
            Vector2 turned = one.Rotate(degrees);
            // Assert
            turned.X.Should().BeApproximately(0, FloatComparasionPrecision);
            turned.Y.Should().BeApproximately(-1, FloatComparasionPrecision);
        }

        [TestMethod()]
        public void Vector2RotatedBy180ShouldBeInverse()
        {
            // Arrange
            Vector2 one = new Vector2(1, 1);
            Angle degrees = new Angle(180f);
            // Act
            Vector2 turned = one.Rotate(degrees);
            // Assert
            turned.X.Should().BeApproximately(-1, FloatComparasionPrecision);
            turned.Y.Should().BeApproximately(-1, FloatComparasionPrecision);
        }

        [TestMethod()]
        public void Vector2RotatedBy0ShouldStaySame()
        {
            // Arrange
            Vector2 one = new Vector2(1, 1);
            Angle degrees = new Angle(0f);
            // Act
            Vector2 turned = one.Rotate(degrees);
            // Assert
            turned.X.Should().BeApproximately(1, FloatComparasionPrecision);
            turned.Y.Should().BeApproximately(1, FloatComparasionPrecision);
        }
    }
}