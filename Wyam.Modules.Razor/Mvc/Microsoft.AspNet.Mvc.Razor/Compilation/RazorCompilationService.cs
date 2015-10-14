﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Razor;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Wyam.Common;
using Wyam.Common.Pipelines;
using Wyam.Modules.Razor.Microsoft.Framework.Internal;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace Wyam.Modules.Razor.Microsoft.AspNet.Mvc.Razor.Compilation
{
    /// <summary>
    /// Default implementation of <see cref="IRazorCompilationService"/>.
    /// </summary>
    public class RazorCompilationService : IRazorCompilationService
    {
        private readonly IMvcRazorHost _razorHost;
        private readonly IExecutionContext _executionContext;

        /// <summary>
        /// Instantiates a new instance of the <see cref="RazorCompilationService"/> class.
        /// </summary>
        /// <param name="compilationService">The <see cref="ICompilationService"/> to compile generated code.</param>
        /// <param name="razorHost">The <see cref="IMvcRazorHost"/> to generate code from Razor files.</param>
        /// <param name="viewEngineOptions">
        /// The <see cref="IFileProvider"/> to read Razor files referenced in error messages.
        /// </param>
        public RazorCompilationService(IExecutionContext executionContext, Type basePageType)
        {
            _executionContext = executionContext;
            _razorHost = new MvcRazorHost(basePageType);
        }

        /// <inheritdoc />
        public Type Compile([NotNull] RelativeFileInfo file)
        {
            GeneratorResults results;
            using (var inputStream = file.FileInfo.CreateReadStream())
            {
                results = GenerateCode(file.RelativePath, inputStream);
            }

            if (!results.Success)
            {
                _executionContext.Trace.Error("{0} errors parsing {1}:{2}{3}", results.ParserErrors.Count(), file.RelativePath, Environment.NewLine, string.Join(Environment.NewLine, results.ParserErrors));
                throw new AggregateException(results.ParserErrors.Select(x => new Exception(x.Message)));
            }

            return Compile(file, results.GeneratedCode);
        }

        /// <summary>
        /// Generate code for the Razor file at <paramref name="relativePath"/> with content
        /// <paramref name="inputStream"/>.
        /// </summary>
        /// <param name="relativePath">
        /// The path of the Razor file relative to the root of the application. Used to generate line pragmas and
        /// calculate the class name of the generated type.
        /// </param>
        /// <param name="inputStream">A <see cref="Stream"/> that contains the Razor content.</param>
        /// <returns>A <see cref="GeneratorResults"/> instance containing results of code generation.</returns>
        protected virtual GeneratorResults GenerateCode(string relativePath, Stream inputStream)
        {
            return _razorHost.GenerateCode(relativePath, inputStream);
        }

        // Cache all MetadataReferences used during compilation
        private static readonly ConcurrentDictionary<string, MetadataReference> _metadataReferences = new ConcurrentDictionary<string, MetadataReference>(); 

        // Wyam - Use the Roslyn scripting engine for compilation
        // In MVC, this part is done in RoslynCompilationService
        private Type Compile([NotNull] RelativeFileInfo file, [NotNull] string compilationContent)
        {
            HashSet<Assembly> assemblies = new HashSet<Assembly>(new AssemblyEqualityComparer())
            {
                Assembly.GetAssembly(typeof (Modules.Razor.Razor))  // Wyam.Modules.Razor
            };
            if (_executionContext.Assemblies != null)
            {
                assemblies.UnionWith(_executionContext.Assemblies);
            }

            var assemblyName = Path.GetRandomFileName();
            var parseOptions = new CSharpParseOptions();
            var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(compilationContent, Encoding.UTF8), parseOptions, assemblyName);
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create(assemblyName, new[] {syntaxTree},
                assemblies.Select(x => _metadataReferences.GetOrAdd(x.Location, y => MetadataReference.CreateFromFile(y))), compilationOptions)
                .AddReferences(
                    // For some reason, Roslyn really wants these added by filename
                    // See http://stackoverflow.com/questions/23907305/roslyn-has-no-reference-to-system-runtime
                    _metadataReferences.GetOrAdd(Path.Combine(assemblyPath, "mscorlib.dll"), x => MetadataReference.CreateFromFile(x)),
                    _metadataReferences.GetOrAdd(Path.Combine(assemblyPath, "System.dll"), x => MetadataReference.CreateFromFile(x)),
                    _metadataReferences.GetOrAdd(Path.Combine(assemblyPath, "System.Core.dll"), x => MetadataReference.CreateFromFile(x)),
                    _metadataReferences.GetOrAdd(Path.Combine(assemblyPath, "System.Runtime.dll"), x => MetadataReference.CreateFromFile(x))
                );
            if (_executionContext.RawConfigAssembly != null && _executionContext.RawConfigAssembly.Length > 0)
            {
                using (MemoryStream memoryStream = new MemoryStream(_executionContext.RawConfigAssembly))
                {
                    compilation = compilation.AddReferences(MetadataReference.CreateFromStream(memoryStream));
                }
            }

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    _executionContext.Trace.Error("{0} errors compiling {1}:{2}{3}", result.Diagnostics.Length, file.RelativePath, Environment.NewLine, string.Join(Environment.NewLine, result.Diagnostics));
                    throw new AggregateException(result.Diagnostics.Select(x => new Exception(x.ToString())));
                }

                ms.Seek(0, SeekOrigin.Begin);
                byte[] assemblyBytes = ms.ToArray();
                Assembly assembly = Assembly.Load(assemblyBytes);

                var type = assembly.GetExportedTypes()
                                    .First(t => t.Name.StartsWith(_razorHost.MainClassNamePrefix, StringComparison.Ordinal));

                return type;
            }
        }

        private class AssemblyEqualityComparer : IEqualityComparer<Assembly>
        {
            public bool Equals(Assembly x, Assembly y)
            {
                return String.CompareOrdinal(x.FullName, y.FullName) == 0;
            }

            public int GetHashCode(Assembly obj)
            {
                return obj.FullName.GetHashCode();
            }
        }
    }
}