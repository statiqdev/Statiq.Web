using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Tracing;

namespace Wyam.Json
{
    /// <summary>
    /// Converts objects stored in metadata or elsewhere to JSON.
    /// </summary>
    /// <remarks>
    /// Generates JSON for a specified object (which can come from document metadata or elsewhere)
    /// and stores it as new content for each input document or in each document's metadata.
    /// </remarks>
    /// <category>Content</category>
    public class GenerateJson : IModule
    {
        private readonly DocumentConfig _data;
        private readonly string _destinationKey;
        private bool _indenting = true;

        /// <summary>
        /// The object stored in metadata at the specified key is converted to JSON, which then either
        /// replaces the content of each input document or is stored in the specified metadata key.
        /// </summary>
        /// <param name="sourceKey">The metadata key of the object to convert to JSON.</param>
        /// <param name="destinationKey">The metadata key where the JSON should be stored (or <c>null</c>
        /// to replace the content of each input document).</param>
        public GenerateJson(string sourceKey, string destinationKey = null)
        {
            _destinationKey = destinationKey;
            _data = (doc, ctx) => doc.Get(sourceKey);
        }

        /// <summary>
        /// The object returned by the specified delegate is converted to JSON, which then either
        /// replaces the content of each input document or is stored in the specified metadata key.
        /// </summary>
        /// <param name="data">A delegate that returns the object to convert to JSON.</param>
        /// <param name="destinationKey">The metadata key where the JSON should be stored (or <c>null</c>
        /// to replace the content of each input document).</param>
        public GenerateJson(ContextConfig data, string destinationKey = null)
        {
            _data = (doc, ctx) => data(ctx);
        }

        /// <summary>
        /// The object returned by the specified delegate is converted to JSON, which then either
        /// replaces the content of each input document or is stored in the specified metadata key.
        /// </summary>
        /// <param name="data">A delegate that returns the object to convert to JSON.</param>
        /// <param name="destinationKey">The metadata key where the JSON should be stored (or <c>null</c>
        /// to replace the content of each input document).</param>
        public GenerateJson(DocumentConfig data, string destinationKey = null)
        {
            _data = data;
        }

        /// <summary>
        /// Specifies whether the generated JSON should be indented.
        /// </summary>
        /// <param name="indenting">If set to <c>true</c>, the JSON is indented.</param>
        public GenerateJson WithIndenting(bool indenting = true)
        {
            _indenting = indenting;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            // Don't use the built-in exception tracing so that we can return the original document on error
            return inputs
               .AsParallel()
               .Select(input =>
               {
                   try
                   {
                       object data = _data(input, context);
                       if (data != null)
                       {
                           string result = JsonConvert.SerializeObject(data,
                               _indenting ? Formatting.Indented : Formatting.None);
                           if (string.IsNullOrEmpty(_destinationKey))
                           {
                               return context.GetDocument(input, result);
                           }
                           return context.GetDocument(input, new MetadataItems
                           {
                               {_destinationKey, result}
                           });
                       }
                   }
                   catch (Exception ex)
                   {
                       Trace.Error("Error serializing JSON for {0}: {1}", input.SourceString(), ex.ToString());
                   }
                   return input;
               })
               .Where(x => x != null);
        }
    }
}
