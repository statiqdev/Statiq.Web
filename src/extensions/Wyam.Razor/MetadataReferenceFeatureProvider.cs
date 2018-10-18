using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.CodeAnalysis;
using Wyam.Common.Util;

namespace Wyam.Razor
{
    internal class MetadataReferenceFeatureProvider : IApplicationFeatureProvider<MetadataReferenceFeature>
    {
        private readonly IList<MetadataReference> _metadataReferences;

        public MetadataReferenceFeatureProvider(IList<MetadataReference> metadataReferences)
        {
            _metadataReferences = metadataReferences;
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