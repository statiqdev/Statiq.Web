// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.FileProviders;
using Wyam.Abstractions;
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
        private readonly string _rootDirectory;
        private readonly IFileProvider _fileProvider;
        private readonly IRazorCompilationService _razorcompilationService;

        public VirtualPathRazorPageFactory(string rootDirectory, IExecutionContext executionContext)
        {
            _rootDirectory = rootDirectory;
            _fileProvider = new PhysicalFileProvider(rootDirectory);
            _razorcompilationService = new RazorCompilationService(executionContext);
        }

        /// <inheritdoc />
        public IRazorPage CreateInstance([NotNull] string relativePath)
        {
            return CreateInstance(relativePath, null);
        }

        public IRazorPage CreateInstance([NotNull] string relativePath, string content)
        {
            if (relativePath.StartsWith("~/", StringComparison.Ordinal))
            {
                // For tilde slash paths, drop the leading ~ to make it work with the underlying IFileProvider.
                relativePath = relativePath.Substring(1);
            }

            // Some of this code is taken from CompilerCache (specifically OnCacheMiss) which is responsible for managing the compilation step in MVC
            // TODO: Add better caching similar to the caching ASP.NET MVC does
            var fileProvider = content == null ? _fileProvider : new DocumentFileProvider(_rootDirectory, content);
            var fileInfo = fileProvider.GetFileInfo(relativePath);
            if (!fileInfo.Exists)
            {
                return null;
            }
            var relativeFileInfo = new RelativeFileInfo(fileInfo, relativePath);
            Type result = _razorcompilationService.Compile(relativeFileInfo);

            var page = (IRazorPage)Activator.CreateInstance(result);
            page.Path = relativePath;

            return page;
        }
    }
}