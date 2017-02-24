using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Tracing;

namespace Wyam.Common.Execution
{
    public static class TraceExceptionsExtensions
    {
        /// <summary>
        /// If an exception is thrown within the action, an error messages will be sent to the trace output
        /// containing information about the document source, the current module, and the exception message.
        /// The exception will also be re-thrown once the message has been sent to the trace listeners.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="document">The document to be processed.</param>
        /// <param name="action">The action to evaluate with the document.</param>
        public static void TraceExceptions(this IExecutionContext context, IDocument document, Action<IDocument> action)
        {
            try
            {
                action(document);
            }
            catch (Exception ex)
            {
                Trace.Error($"Exception while processing document {document?.SourceString() ?? "unknown"} in module {context?.Module?.GetType().Name ?? "unknown"}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// If an exception is thrown within the action, an error messages will be sent to the trace output
        /// containing information about the document source, the current module, and the exception message.
        /// The exception will also be re-thrown once the message has been sent to the trace listeners.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="document">The document to be processed.</param>
        /// <param name="func">The function to evaluate with the document.</param>
        public static TResult TraceExceptions<TResult>(this IExecutionContext context, IDocument document, Func<IDocument, TResult> func)
        {
            try
            {
                return func(document);
            }
            catch (Exception ex)
            {
                Trace.Error($"Exception while processing document {document?.SourceString() ?? "unknown"} in module {context?.Module?.GetType().Name ?? "unknown"}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// If an exception is thrown within the action, an error messages will be sent to the trace output
        /// containing information about the current module and the exception message.
        /// The exception will also be re-thrown once the message has been sent to the trace listeners.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="action">The action to evaluate.</param>
        public static void TraceExceptions(this IExecutionContext context, Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Trace.Error($"Exception while processing in module {context?.Module?.GetType().Name ?? "unknown"}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// If an exception is thrown within the action, an error messages will be sent to the trace output
        /// containing information about the current module and the exception message.
        /// The exception will also be re-thrown once the message has been sent to the trace listeners.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="func">The function to evaluate.</param>
        public static TResult TraceExceptions<TResult>(this IExecutionContext context, Func<TResult> func)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                Trace.Error($"Exception while processing in module {context?.Module?.GetType().Name ?? "unknown"}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// If an exception is thrown within the action, an error messages will be sent to the trace output
        /// containing information about the document source, the current module, and the exception message.
        /// The exception will also be re-thrown once the message has been sent to the trace listeners.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="documents">The documents to be processed.</param>
        /// <param name="action">The action to evaluate with the documents.</param>
        public static void ForEach(this IExecutionContext context, IEnumerable<IDocument> documents, Action<IDocument> action)
        {
            foreach (IDocument document in documents)
            {
                TraceExceptions(context, document, action);
            }
        }

        /// <summary>
        /// If an exception is thrown within the action, an error messages will be sent to the trace output
        /// containing information about the document source, the current module, and the exception message.
        /// The exception will also be re-thrown once the message has been sent to the trace listeners.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="documents">The documents to be processed.</param>
        /// <param name="action">The action to evaluate with the documents.</param>
        public static void ParallelForEach(this IExecutionContext context, IEnumerable<IDocument> documents, Action<IDocument> action)
        {
            Parallel.ForEach(documents, document =>
            {
                TraceExceptions(context, document, action);
            });
        }

        public static IEnumerable<TResult> Select<TResult>(
            this IEnumerable<IDocument> source, IExecutionContext context, Func<IDocument, TResult> selector) =>
                source.Select(x => context.TraceExceptions(x, selector));

        public static IEnumerable<TResult> SelectMany<TResult>(
            this IEnumerable<IDocument> source, IExecutionContext context, Func<IDocument, IEnumerable<TResult>> selector) =>
                source.SelectMany(x => context.TraceExceptions(x, selector));

        public static IEnumerable<IDocument> Where(
            this IEnumerable<IDocument> source, IExecutionContext context, Func<IDocument, bool> predicate) =>
                source.Where(x => context.TraceExceptions(x, predicate));

        public static ParallelQuery<IDocument> Select(
            this ParallelQuery<IDocument> query, IExecutionContext context, Func<IDocument, IDocument> selector) =>
                query.Select(x => context.TraceExceptions(x, selector));

        public static ParallelQuery<IDocument> SelectMany(
            this ParallelQuery<IDocument> query, IExecutionContext context, Func<IDocument, IEnumerable<IDocument>> selector) =>
                query.SelectMany(x => context.TraceExceptions(x, selector));
    }
}
