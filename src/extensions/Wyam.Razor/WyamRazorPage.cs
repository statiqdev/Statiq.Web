using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Tracing;

namespace Wyam.Razor
{
    public abstract class WyamRazorPage<TModel> : RazorPage<TModel>
    {
        public IDocument Document => ViewData[ViewDataKeys.WyamDocument] as IDocument;
        public IMetadata Metadata => Document;

        public IExecutionContext ExecutionContext => ViewData[ViewDataKeys.WyamExecutionContext] as IExecutionContext;
        public new IExecutionContext Context => ExecutionContext;
        public HttpContext HttpContext => base.Context;

        public IDocumentCollection Documents => ExecutionContext.Documents;

        public ITrace Trace => Common.Tracing.Trace.Current;
    }
}