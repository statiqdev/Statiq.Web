using System;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Configuration
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
                    $"Could not convert from type {value?.GetType().Name ?? "null"} to type {typeof(T).Name}");
            }
            return result;
        }

        public static T TryInvoke<T>(this ContextConfig config, IExecutionContext context)
        {
            object value = config(context);
            T result;
            return context.TryConvert(value, out result) ? result : default(T);
        }

        public static T Invoke<T>(this DocumentConfig config, IDocument document, IExecutionContext context)
        {
            object value = config(document, context);
            T result;
            if (!context.TryConvert(value, out result))
            {
                throw new InvalidOperationException(
                    $"Could not convert from type {value?.GetType().Name ?? "null"} to type {typeof(T).Name}");
            }
            return result;
        }

        public static T TryInvoke<T>(this DocumentConfig config, IDocument document, IExecutionContext context)
        {
            object value = config(document, context);
            T result;
            return context.TryConvert(value, out result) ? result : default(T);
        }
    }
}
