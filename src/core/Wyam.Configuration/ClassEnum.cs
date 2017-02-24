using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Wyam.Configuration
{
    /// <summary>
    /// A base class that can be used to create more powerful "enum" classes that
    /// use fields to store the values, which are instances of the class.
    /// </summary>
    /// <typeparam name="T">The derived class type.</typeparam>
    public abstract class ClassEnum<T> where T : ClassEnum<T>
    {
        static ClassEnum()
        {
            Values = typeof(T)
                .GetFields(BindingFlags.Static | BindingFlags.Public)
                .Where(x => x.FieldType == typeof(T))
                .ToDictionary(x => x.Name, x => (T)x.GetValue(null), StringComparer.OrdinalIgnoreCase);
        }

        public static IReadOnlyDictionary<string, T> Values { get; }
    }
}