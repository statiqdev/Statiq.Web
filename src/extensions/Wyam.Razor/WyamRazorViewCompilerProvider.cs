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

    /// <summary>
    /// This is copied from <see cref="RazorViewCompilerProvider"/> and exists entirely to provide
    /// <see cref="WyamRazorViewCompiler"/> instead of <see cref="RazorViewCompiler"/>.
    /// </summary>
    internal class WyamRazorViewCompilerProvider : IViewCompilerProvider
    {
        private object _initializeLock = new object();
        private readonly RazorProjectEngine _razorProjectEngine;
        private readonly ApplicationPartManager _applicationPartManager;
        private readonly IRazorViewEngineFileProviderAccessor _fileProviderAccessor;
        private readonly CSharpCompiler _csharpCompiler;
        private readonly RazorViewEngineOptions _viewEngineOptions;
        private readonly ILogger<RazorViewCompiler> _logger;
        private readonly Func<IViewCompiler> _createCompiler;
        private bool _initialized;
        private IViewCompiler _compiler;

        public WyamRazorViewCompilerProvider(ApplicationPartManager applicationPartManager, RazorProjectEngine razorProjectEngine, IRazorViewEngineFileProviderAccessor fileProviderAccessor, CSharpCompiler csharpCompiler, IOptions<RazorViewEngineOptions> viewEngineOptionsAccessor, ILoggerFactory loggerFactory)
        {
            _applicationPartManager = applicationPartManager;
            _razorProjectEngine = razorProjectEngine;
            _fileProviderAccessor = fileProviderAccessor;
            _csharpCompiler = csharpCompiler;
            _viewEngineOptions = viewEngineOptionsAccessor.Value;
            _logger = loggerFactory.CreateLogger<RazorViewCompiler>();
            _createCompiler = new Func<IViewCompiler>(CreateCompiler);
        }

        public IViewCompiler GetCompiler()
        {
            if (_fileProviderAccessor.FileProvider is NullFileProvider)
            {
                throw new InvalidOperationException();
            }
            return LazyInitializer.EnsureInitialized<IViewCompiler>(ref _compiler, ref _initialized, ref _initializeLock, _createCompiler);
        }

        private IViewCompiler CreateCompiler()
        {
            ViewsFeature feature = new ViewsFeature();
            _applicationPartManager.PopulateFeature<ViewsFeature>(feature);
            return (IViewCompiler)new WyamRazorViewCompiler(_fileProviderAccessor.FileProvider, _razorProjectEngine, _csharpCompiler, _viewEngineOptions.CompilationCallback, feature.ViewDescriptors, (ILogger)_logger);
        }
    }
}