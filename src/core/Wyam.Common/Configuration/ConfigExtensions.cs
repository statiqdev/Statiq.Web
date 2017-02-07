using System;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Configuration
{
    public static class ConfigExtensions
    {
        public static T Invoke<T>(this ContextConfig config, IExecutionContext context)
        {
            return Invoke<T>(config, context, null);
        }

        public static T Invoke<T>(this ContextConfig config, IExecutionContext context, string errorDetails)
        {
            object value = config(context);
            T result;
            if (!context.TryConvert(value, out result))
            {
                errorDetails = GetErrorDetails(errorDetails);
                throw new InvalidOperationException(
                    $"Could not convert from type {value?.GetType().Name ?? "null"} to type {typeof(T).Name}{errorDetails}");
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
            return Invoke<T>(config, document, context, null);
        }

        public static T Invoke<T>(this DocumentConfig config, IDocument document, IExecutionContext context, string errorDetails)
        {
            object value = config(document, context);
            T result;
            if (!context.TryConvert(value, out result))
            {
                errorDetails = GetErrorDetails(errorDetails);
                throw new InvalidOperationException(
                    $"Could not convert from type {value?.GetType().Name ?? "null"} to type {typeof(T).Name} for {document.SourceString()}{errorDetails}");
            }
            return result;
        }

        public static T TryInvoke<T>(this DocumentConfig config, IDocument document, IExecutionContext context)
        {
            object value = config(document, context);
            T result;
            return context.TryConvert(value, out result) ? result : default(T);
        }

        private static string GetErrorDetails(string errorDetails)
        {
            if (errorDetails != null && !errorDetails.StartsWith(" "))
            {
                errorDetails = " " + errorDetails;
            }
            return errorDetails;
        }
    }
}
