using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.CodeAnalysis;
using Wyam.Common.Execution;

namespace Wyam.Razor
{
    internal class MetadataReferenceFeatureProvider : IApplicationFeatureProvider<MetadataReferenceFeature>
    {
        private readonly IExecutionContext _executionContext;

        public MetadataReferenceFeatureProvider(IExecutionContext executionContext)
        {
            _executionContext = executionContext;
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
            foreach (byte[] image in _executionContext.DynamicAssemblies)
            {
                feature.MetadataReferences.Add(MetadataReference.CreateFromImage(image));
            }
        }
    }
}