using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Wyam.Common.Util;

namespace Wyam.Razor
{
    internal class WyamRazorViewCompiler : RazorViewCompiler
    {
        public WyamRazorViewCompiler(
            IFileProvider fileProvider,
            RazorProjectEngine projectEngine,
            CSharpCompiler csharpCompiler,
            Action<RoslynCompilationContext> compilationCallback,
            IList<CompiledViewDescriptor> precompiledViews,
            ILogger logger)
            : base(
                  fileProvider,
                  projectEngine,
                  csharpCompiler,
                  compilationCallback,
                  precompiledViews,
                  logger)
        {
        }

        protected override CompiledViewDescriptor CompileAndEmit(string relativePath)
        {
            CompiledViewDescriptor descriptor = base.CompileAndEmit(relativePath);

            // The Razor compiler adds attributes to the generated IRazorPage code that provide the relative path of the page
            // but since Wyam uses "invisible" input path(s) that appear in the physical file system but not the virtual one,
            // we have to remove the input path from the start of the relative path - otherwise we'll end up looking for nested
            // views in locations like "/input/input/_foo.cshtml"
            if (descriptor.RelativePath.EndsWith(relativePath))
            {
                descriptor.RelativePath = relativePath;
            }

            return descriptor;
        }
    }
}