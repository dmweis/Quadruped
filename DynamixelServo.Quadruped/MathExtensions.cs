using System;

namespace DynamixelServo.Quadruped
{
    static class MathExtensions
    {
        public static double RadToDegree(this double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        public static double DegreeToRad(this double angle)
        {
            return Math.PI * angle / 180.0;
        }

        public static double ToPower(this double number, double powerOf)
        {
            return Math.Pow(number, powerOf);
        }
    }
}
