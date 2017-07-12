using System;
using DynamixelServo.Driver;

namespace DynamixelServo.Quadruped
{
    public struct Angle : IComparable<Angle>
    {
        private readonly float _value;

        public Angle(float value)
        {
            _value = Mod(value, 360f);
        }

        public static explicit operator Angle(float angle)
        {
            return new Angle(angle);
        }

        public static implicit operator float(Angle angle)
        {
            return angle._value;
        }

        public static Angle operator +(Angle a, Angle b)
        {
            return (Angle)(a._value + b._value);
        }

        public static Angle operator -(Angle a, Angle b)
        {
            return (Angle) (a._value - b._value);
        }

        public float TranslateTo180()
        {
            return _value - 180;
        }

        public ushort ToDynamixelUnits()
        {
            return DynamixelDriver.DegreesToUnits(new Angle(_value + 150f));
        }

        public int CompareTo(Angle other)
        {
            return _value.CompareTo(other._value);
        }

        public override string ToString()
        {
            return _value + "\u00B0";
        }

        private static float Mod(float divident, float divisor)
        {
            float modulo = divident % divisor;
            return modulo < 0 ? modulo + divisor : modulo;
        }
    }
}
