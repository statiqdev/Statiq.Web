using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Tracing;

namespace Wyam.Common.Execution
{
    /// <summary>
    /// Extensions to send exception messages to trace output with relevant context such as currently executing module and document.
    /// </summary>
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
        /// <typeparam name="TResult">The return type of the function.</typeparam>
        /// <param name="context">The current execution context.</param>
        /// <param name="document">The document to be processed.</param>
        /// <param name="func">The function to evaluate with the document.</param>
        /// <returns>The result of the function.</returns>
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
        /// <typeparam name="TResult">The return type of the function.</typeparam>
        /// <param name="context">The current execution context.</param>
        /// <param name="func">The function to evaluate.</param>
        /// <returns>The result of the function.</returns>
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

        /// <summary>
        /// Evaluates a LINQ <c>Select</c> method and traces any exceptions.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="selector">The selector function.</param>
        /// <returns>The result sequence.</returns>
        public static IEnumerable<TResult> Select<TResult>(
            this IEnumerable<IDocument> source, IExecutionContext context, Func<IDocument, TResult> selector) =>
                source.Select(x => context.TraceExceptions(x, selector));

        /// <summary>
        /// Evaluates a LINQ <c>SelectMany</c> method and traces any exceptions.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="selector">The selector function.</param>
        /// <returns>The result sequence.</returns>
        public static IEnumerable<TResult> SelectMany<TResult>(
            this IEnumerable<IDocument> source, IExecutionContext context, Func<IDocument, IEnumerable<TResult>> selector) =>
                source.SelectMany(x => context.TraceExceptions(x, selector));

        /// <summary>
        /// Evaluates a LINQ <c>Where</c> method and traces any exceptions.
        /// </summary>
        /// <param name="source">The source sequence.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="predicate">The predicate function.</param>
        /// <returns>The result sequence.</returns>
        public static IEnumerable<IDocument> Where(
            this IEnumerable<IDocument> source, IExecutionContext context, Func<IDocument, bool> predicate) =>
                source.Where(x => context.TraceExceptions(x, predicate));

        /// <summary>
        /// Evaluates a PLINQ <c>Select</c> method over a sequence of <see cref="IDocument"/> and traces any exceptions.
        /// </summary>
        /// <param name="query">The source query.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="selector">The selector function.</param>
        /// <returns>The result query.</returns>
        public static ParallelQuery<IDocument> Select(
            this ParallelQuery<IDocument> query, IExecutionContext context, Func<IDocument, IDocument> selector) =>
                query.Select(x => context.TraceExceptions(x, selector));

        /// <summary>
        /// Evaluates a PLINQ <c>SelectMany</c> method over a sequence of <see cref="IDocument"/> and traces any exceptions.
        /// </summary>
        /// <param name="query">The source query.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="selector">The selector function.</param>
        /// <returns>The result query.</returns>
        public static ParallelQuery<IDocument> SelectMany(
            this ParallelQuery<IDocument> query, IExecutionContext context, Func<IDocument, IEnumerable<IDocument>> selector) =>
                query.SelectMany(x => context.TraceExceptions(x, selector));
    }
}
