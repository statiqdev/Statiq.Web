using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.CodeAnalysis;

namespace Wyam.Razor
{
    internal class MetadataReferenceFeatureProvider : IApplicationFeatureProvider<MetadataReferenceFeature>
    {
        private readonly DynamicAssemblyCollection _dynamicAssemblies;

        public MetadataReferenceFeatureProvider(DynamicAssemblyCollection dynamicAssemblies)
        {
            _dynamicAssemblies = dynamicAssemblies;
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

            // Add all references from the execution context
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !x.IsDynamic && !string.IsNullOrEmpty(x.Location)))
            {
                feature.MetadataReferences.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
            foreach (byte[] image in _dynamicAssemblies ?? Enumerable.Empty<byte[]>())
            {
                feature.MetadataReferences.Add(MetadataReference.CreateFromImage(image));
            }
        }
    }
}