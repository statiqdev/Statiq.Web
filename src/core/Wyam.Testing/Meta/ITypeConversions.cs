using System;
using System.Collections.Generic;
using System.Text;

namespace Wyam.Testing.Meta
{
    /// <summary>
    /// Provides simple type conversions for use in tests.
    /// </summary>
    public interface ITypeConversions
    {
        Dictionary<(Type Value, Type Result), Func<object, object>> TypeConversions { get; }

        void AddTypeConversion<T, TResult>(Func<T, TResult> typeConversion);
    }
}
