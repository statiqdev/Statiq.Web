using System;
using System.Collections.Generic;
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
        public RazorHost(NamespaceCollection namespaces, IChunkTreeCache chunkTreeCache, ITagHelperDescriptorResolver resolver, IBasePageTypeProvider basePageTypeProvider)
            : base(chunkTreeCache, resolver)
        {
            // Remove the backtick from generic class names
            string baseClassName = basePageTypeProvider.BasePageType.FullName;
            int tickIndex = baseClassName.IndexOf('`');
            if (tickIndex > 0)
            {
                baseClassName = baseClassName.Substring(0, tickIndex);
            }

            DefaultBaseClass = basePageTypeProvider.BasePageType.IsGenericTypeDefinition ? $"{baseClassName}<{ChunkHelper.TModelToken}>" : baseClassName;
            DefaultInheritedChunks.OfType<SetBaseTypeChunk>().First().TypeName = DefaultBaseClass;  // The chunk is actually what injects the base name into the view
            EnableInstrumentation = false;

            // Add additional default namespaces from the execution context
            foreach (string ns in namespaces)
            {
                NamespaceImports.Add(ns);
            }
        }

        public RazorHost(string root)
            : base(root)
        {
            throw new NotSupportedException();
        }

        public override string DefaultModel => "IDocument";
    }
}