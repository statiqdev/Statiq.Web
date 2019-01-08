using System;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Configuration
{
    /// <summary>
    /// Extensions for dealing with config delegates.
    /// </summary>
    public static class ConfigExtensions
    {
        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        /// <typeparam name="T">The desired result type.</typeparam>
        /// <param name="config">The delegate.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>A typed result from invoking the delegate.</returns>
        public static T Invoke<T>(this ContextConfig config, IExecutionContext context)
        {
            return Invoke<T>(config, context, null);
        }

        /// <summary>
        /// Invokes the delegate with additional information in the exception message if the conversion fails.
        /// </summary>
        /// <typeparam name="T">The desired result type.</typeparam>
        /// <param name="config">The delegate.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="errorDetails">A string to add to the exception message should the conversion fail.</param>
        /// <returns>A typed result from invoking the delegate.</returns>
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

        /// <summary>
        /// Attempts to invoke the delegate and returns a default value of <typeparamref name="T"/> if the conversion fails.
        /// </summary>
        /// <typeparam name="T">The desired result type.</typeparam>
        /// <param name="config">The delegate.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>A typed result from invoking the delegate, or the default value of <typeparamref name="T"/> if the conversion fails.</returns>
        public static T TryInvoke<T>(this ContextConfig config, IExecutionContext context)
        {
            object value = config(context);
            T result;
            return context.TryConvert(value, out result) ? result : default(T);
        }

        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        /// <typeparam name="T">The desired result type.</typeparam>
        /// <param name="config">The delegate.</param>
        /// <param name="document">The document.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>A typed result from invoking the delegate.</returns>
        public static T Invoke<T>(this DocumentConfig config, IDocument document, IExecutionContext context)
        {
            return Invoke<T>(config, document, context, null);
        }

        /// <summary>
        /// Invokes the delegate with additional information in the exception message if the conversion fails.
        /// </summary>
        /// <typeparam name="T">The desired result type.</typeparam>
        /// <param name="config">The delegate.</param>
        /// <param name="document">The document.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="errorDetails">A string to add to the exception message should the conversion fail.</param>
        /// <returns>A typed result from invoking the delegate.</returns>
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

        /// <summary>
        /// Attempts to invoke the delegate and returns a default value of <typeparamref name="T"/> if the conversion fails.
        /// </summary>
        /// <typeparam name="T">The desired result type.</typeparam>
        /// <param name="config">The delegate.</param>
        /// <param name="document">The document.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>A typed result from invoking the delegate, or the default value of <typeparamref name="T"/> if the conversion fails.</returns>
        public static T TryInvoke<T>(this DocumentConfig config, IDocument document, IExecutionContext context)
        {
            object value = config(document, context);
            T result;
            return context.TryConvert(value, out result) ? result : default(T);
        }

        private static string GetErrorDetails(string errorDetails)
        {
            if (errorDetails?.StartsWith(" ") == false)
            {
                errorDetails = " " + errorDetails;
            }
            return errorDetails;
        }
    }
}
