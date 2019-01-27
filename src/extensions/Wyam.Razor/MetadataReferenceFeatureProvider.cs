using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.CodeAnalysis;
using Wyam.Common.Util;

namespace Wyam.Razor
{
    internal class MetadataReferenceFeatureProvider : IApplicationFeatureProvider<MetadataReferenceFeature>
    {
        private readonly IList<MetadataReference> _metadataReferences;

        public MetadataReferenceFeatureProvider(DynamicAssemblyCollection dynamicAssemblies)
        {
            _metadataReferences = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !x.IsDynamic && !string.IsNullOrEmpty(x.Location))
                .Select(x => MetadataReference.CreateFromFile(x.Location))
                .Concat((dynamicAssemblies ?? Enumerable.Empty<byte[]>())
                    .Select(x => (MetadataReference)MetadataReference.CreateFromImage(x)))
                .Concat(new MetadataReference[]
                {
                    // Razor/MVC assemblies that might not be loaded yet
                    MetadataReference.CreateFromFile(typeof(IHtmlContent).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("Microsoft.CSharp")).Location)
                })
                .ToList();
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, MetadataReferenceFeature feature)
        {
            if (parts == null)
            {
                throw new ArgumentNullException(nameof(parts));
            }

            if (feature == null)
            {
                throw new ArgumentNullException(nameof(feature));
            }

            feature.MetadataReferences.AddRange(_metadataReferences);
        }
    }
}