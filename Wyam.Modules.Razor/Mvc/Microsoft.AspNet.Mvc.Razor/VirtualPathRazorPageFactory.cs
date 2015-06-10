// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Policy;
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
        private readonly Dictionary<string, CacheEntry> _pageCache = new Dictionary<string, CacheEntry>(); 

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

            // Code below is taken from CompilerCache (specifically OnCacheMiss) which is responsible for managing the compilation step in MVC

            // Check the file
            var fileProvider = content == null ? _fileProvider : new DocumentFileProvider(_rootDirectory, content);
            var fileInfo = fileProvider.GetFileInfo(relativePath);
            if (!fileInfo.Exists)
            {
                return null;
            }

            // Check the cache
            CacheEntry cacheEntry;
            string hash = RazorFileHash.GetHash(fileInfo);
            if (_pageCache.TryGetValue(relativePath, out cacheEntry))
            {
                // It was found in the cache, see if it still matches
                if (cacheEntry.Length == fileInfo.Length
                    && cacheEntry.LastModified == fileInfo.LastModified
                    && cacheEntry.Hash == hash)
                {
                    return cacheEntry.Page;
                }
            }

            // Compile and store in cache
            var relativeFileInfo = new RelativeFileInfo(fileInfo, relativePath);
            Type result = _razorcompilationService.Compile(relativeFileInfo);
            IRazorPage page = (IRazorPage)Activator.CreateInstance(result);
            page.Path = relativePath;
            _pageCache[relativePath] = new CacheEntry
            {
                Page = page,
                Length = fileInfo.Length,
                LastModified = fileInfo.LastModified,
                Hash = hash
            };
            return page;
        }

        private class CacheEntry
        {
            public IRazorPage Page { get; set; }
            public long Length { get; set; }
            public DateTimeOffset LastModified { get; set; }
            public string Hash { get; set; }
        }
    }
}