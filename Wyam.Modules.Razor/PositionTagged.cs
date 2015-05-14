using System;

namespace Wyam.Modules.Razor
{

    // From https://github.com/aspnet/Mvc/blob/dev/src/Microsoft.AspNet.Mvc.Razor/PositionTagged.cs
    public class PositionTagged<T>
    {
        public PositionTagged(T value, int offset)
        {
            Position = offset;
            Value = value;
        }

        public int Position { get; private set; }

        public T Value { get; private set; }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator T(PositionTagged<T> value)
        {
            return value.Value;
        }

        public static implicit operator PositionTagged<T>(Tuple<T, int> value)
        {
            return new PositionTagged<T>(value.Item1, value.Item2);
        }
    }
}