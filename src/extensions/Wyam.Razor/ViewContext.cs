using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Razor
{
    internal class ViewContext : Microsoft.AspNetCore.Mvc.Rendering.ViewContext
    {
        public ViewContext(ActionContext actionContext, IView view, ViewDataDictionary viewData, 
            ITempDataDictionary tempData, TextWriter writer, HtmlHelperOptions htmlHelperOptions,
            IDocument document, IExecutionContext executionContext) 
            : base(actionContext, view, viewData, tempData, writer, htmlHelperOptions)
        {
            viewData[ViewDataKeys.WyamDocument] = document;
            viewData[ViewDataKeys.WyamExecutionContext] = executionContext;
        }
    }
}