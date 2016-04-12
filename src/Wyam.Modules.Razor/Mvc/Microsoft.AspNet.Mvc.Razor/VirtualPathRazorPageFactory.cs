// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using Microsoft.AspNet.FileProviders;
using Wyam.Common;
using Wyam.Common.Caching;
using Wyam.Common.Execution;
using Wyam.Modules.Razor.Microsoft.AspNet.Mvc.Razor.Compilation;
using Wyam.Modules.Razor.Microsoft.Framework.Internal;

namespace Wyam.Modules.Razor.Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Represents a <see cref="IRazorPageFactory"/> that creates <see cref="RazorPage"/> instances
    /// from razor files in the file system.
    /// </summary>
    public class VirtualPathRazorPageFactory : IRazorPageFactory
    {
        private readonly WyamFileProvider _fileProvider;
        private readonly IRazorCompilationService _razorcompilationService;
        private readonly IExecutionContext _executionContext;

        public VirtualPathRazorPageFactory(IExecutionContext executionContext, Type basePageType)
        {
            _fileProvider = new WyamFileProvider(executionContext.FileSystem);
            _razorcompilationService = new RazorCompilationService(executionContext, basePageType);
            _executionContext = executionContext;
        }

        /// <inheritdoc />
        public IRazorPage CreateInstance([NotNull] string relativePath)
        {
            return CreateInstance(relativePath, null);
        }

        public IRazorPage CreateInstance([NotNull] string relativePath, Stream stream)
        {
            if (relativePath.StartsWith("~/", StringComparison.Ordinal))
            {
                // For tilde slash paths, drop the leading ~ to make it work with the underlying IFileProvider.
                relativePath = relativePath.Substring(1);
            }

            // Code below is taken from CompilerCache (specifically OnCacheMiss) which is responsible for managing the compilation step in MVC

            // Check the file
            var fileProvider = stream == null ? (IFileProvider)_fileProvider : new WyamStreamFileProvider(_fileProvider, stream);
            var fileInfo = fileProvider.GetFileInfo(relativePath);
            if (!fileInfo.Exists)
            {
                return null;
            }
            
            // If relative path is the root, it probably means this isn't from reading a file so don't bother with caching
            IExecutionCache cache = relativePath == "/" ? null : _executionContext.ExecutionCache;
            string key = null;
            Type pageType = null;
            if (cache != null)
            {
                key = relativePath + " " + RazorFileHash.GetHash(fileInfo);
                if (!cache.TryGetValue(key, out pageType))
                {
                    pageType = null;
                }
            }

            // Compile and store in cache if not found
            if (pageType == null)
            {
                var relativeFileInfo = new RelativeFileInfo(fileInfo, relativePath);
                pageType = _razorcompilationService.Compile(relativeFileInfo);
                cache?.Set(key, pageType);
            }

            // Create an return a new page instance
            IRazorPage page = (IRazorPage)Activator.CreateInstance(pageType);
            page.Path = relativePath;
            return page;
        }
    }
}