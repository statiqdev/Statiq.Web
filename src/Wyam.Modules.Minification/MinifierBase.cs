using System;
using System.Collections.Generic;
using System.Linq;
using WebMarkupMin.Core;
using Wyam.Common.Documents;
using Wyam.Common.Pipelines;
using Wyam.Common.Tracing;

namespace Wyam.Modules.Minification
{
    public abstract class MinifierBase
    {
        public IEnumerable<IDocument> Minify(IReadOnlyList<IDocument> inputs, IExecutionContext context, Func<string, MinificationResultBase> minify, string minifierType)
        {
            return inputs.AsParallel().Select(input =>
            {
                try
                {
                    MinificationResultBase result = minify(input.Content);

                    if (result.Errors.Count > 0)
                    {
                        Trace.Error("{0} errors found while minifying {4} for {1}:{2}{3}", result.Errors.Count, input.Source, Environment.NewLine, string.Join(Environment.NewLine, result.Errors.Select(x => MinificationErrorInfoToString(x))), minifierType);
                        return input;
                    }

                    if (result.Warnings.Count > 0)
                    {
                        Trace.Warning("{0} warnings found while minifying {4} for {1}:{2}{3}", result.Warnings.Count, input.Source, Environment.NewLine, string.Join(Environment.NewLine, result.Warnings.Select(x => MinificationErrorInfoToString(x))), minifierType);
                    }

                    return context.GetDocument(input, result.MinifiedContent);
                }
                catch (Exception ex)
                {
                    Trace.Error("Exception while minifying {2} for {0}: {1}", input.Source, ex.Message, minifierType);
                    return input;
                }
            });
        }

        private string MinificationErrorInfoToString(MinificationErrorInfo info)
        {
            return string.Format("Line {0}, Column {1}:{5}{2} {3}{5}{4}", info.LineNumber, info.ColumnNumber, info.Category, info.Message, info.SourceFragment, Environment.NewLine);
        }
    }
}