// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;
using Microsoft.CodeAnalysis.Text;
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

        public RazorCompilationService()
        {
            _razorHost = new MvcRazorHost();
        }

        /// <inheritdoc />
        public Type Compile([NotNull] RelativeFileInfo file)
        {
            GeneratorResults results;
            using (var inputStream = file.FileInfo.CreateReadStream())
            {
                results = _razorHost.GenerateCode(file.RelativePath, inputStream);
            }

            if (!results.Success)
            {
                throw new AggregateException(results.ParserErrors.Select(x => new Exception(x.Message)));
            }

            return Compile(results.GeneratedCode);
        }

        // Use the Roslyn scripting engine for compilation - in MVC, this part is done in RoslynCompilationService
        private Type Compile([NotNull] string compilationContent)
        {
            // TODO: Get assemblies from Wyam engine
            HashSet<Assembly> assemblies = new HashSet<Assembly>(new AssemblyEqualityComparer())
            {
                Assembly.GetAssembly(typeof (object)),  // System
                Assembly.GetAssembly(typeof (System.Collections.Generic.List<>)),  // System.Collections.Generic 
                Assembly.GetAssembly(typeof (System.Linq.ImmutableArrayExtensions)),  // System.Linq
                Assembly.GetAssembly(typeof (System.Dynamic.DynamicObject)), // System.Core (needed for dynamic)
                Assembly.GetAssembly(typeof (Binder)), // Microsoft.CSharp (needed for dynamic)
                Assembly.GetAssembly(typeof (System.IO.Path)), // System.IO
                Assembly.GetAssembly(typeof (Wyam.Core.Engine)), // Wyam.Core
                Assembly.GetAssembly(typeof (Wyam.Abstractions.IModule)), // Wyam.Abstractions
                Assembly.GetAssembly(typeof (Modules.Razor.Razor))  // Wyam.Modules.Razor
            };
            
            var assemblyName = Path.GetRandomFileName();
            var parseOptions = new CSharpParseOptions();
            var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(compilationContent, Encoding.UTF8), parseOptions, assemblyName);
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create(assemblyName, new[] {syntaxTree},
                assemblies.Select(MetadataReference.CreateFromAssembly), compilationOptions)
                .AddReferences(
                    // For some reason, Roslyn really wants these added by filename
                    // See http://stackoverflow.com/questions/23907305/roslyn-has-no-reference-to-system-runtime
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll"))
                );

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
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