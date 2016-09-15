using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Directives;
using Microsoft.AspNetCore.Razor.Chunks;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Wyam.Common.Execution;

namespace Wyam.Razor
{
    internal class RazorHost : MvcRazorHost
    {
        public RazorHost(IExecutionContext executionContext, IChunkTreeCache chunkTreeCache, ITagHelperDescriptorResolver resolver) : base(chunkTreeCache, resolver)
        {
            DefaultBaseClass = "Wyam.Razor.RazorPage";
            DefaultInheritedChunks.OfType<SetBaseTypeChunk>().First().TypeName = DefaultBaseClass;  // The chunk is actually what injects the base name into the view
            EnableInstrumentation = false;

            // Add additional default namespaces from the execution context
            foreach (string ns in executionContext.Namespaces)
            {
                NamespaceImports.Add(ns);
            }
        }

        public RazorHost(string root) : base(root)
        {
            throw new NotSupportedException();
        }

        public override string DefaultModel => "IDocument";
    }
}