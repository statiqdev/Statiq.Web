using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common
{
    public static class ConfigExtensions
    {
        public static T Invoke<T>(this ContextConfig config, IExecutionContext context)
        {
            object value = config(context);
            T result;
            if (!context.TryConvert(value, out result))
            {
                throw new InvalidOperationException(
                    string.Format("Could not convert from type {0} to type {1}", value?.GetType().Name ?? "null", typeof(T).Name));
            }
            return result;
        }

        public static T Invoke<T>(this DocumentConfig config, IDocument document, IExecutionContext context)
        {
            object value = config(document, context);
            T result;
            if (!context.TryConvert(value, out result))
            {
                throw new InvalidOperationException(
                    string.Format("Could not convert from type {0} to type {1}", value?.GetType().Name ?? "null", typeof(T).Name));
            }
            return result;
        }
    }
}
