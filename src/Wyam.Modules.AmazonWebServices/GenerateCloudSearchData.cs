using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Modules.AmazonWebServices
{
    /// <summary>
    /// Generates bulk JSON upload data to get documents into Amazon CloudSearch.
    /// </summary>
    /// <remarks>
    /// This module creates a single document from a pipeline with JSON data containing the correctly formatted commands for Amazon CloudSearch.
    /// Note that this just creates the document. Once that document is written to the file system, you will still need to upload the document
    /// to a correctly configured CloudSearch instance using the API or Amazon CLI.
    /// </remarks>
    /// <category>Amazon Web Services</category>
    public class GenerateCloudSearchData : IModule
    {
        private readonly string _idMetaKey;
        private readonly string _writePath;
        private readonly string _bodyField;
        private Dictionary<string, string> _fieldMap;


        /// <summary>
        /// Generates Amazon CloudSearch JSON data.
        /// </summary>
        /// <param name="idMetaKey">The meta key represeting the unique ID for this document.  If NULL, the Document.Id will be used.</param>
        /// <param name="bodyField">The field name for the document contents.  If NULL, the document contents will not be written to the data.</param>
        public GenerateCloudSearchData(string idMetaKey, string bodyField)
        {
            _idMetaKey = idMetaKey;
            _bodyField = bodyField;
            _fieldMap = new Dictionary<string, string>();
        }

        /// <summary>
        /// Adds a mapping from meta key to CloudSearch field. When provided, the contents of the meta key will be written to the profided field name.
        /// </summary>
        /// <param name="fieldName">The CloudSearch field name.</param>
        /// <param name="metaKey">The meta key.</param>
        public GenerateCloudSearchData MapMetaField(string fieldName, string metaKey)
        {
            if (fieldName == null)
            {
                throw new ArgumentNullException(nameof(fieldName));
            }
            if (metaKey == null)
            {
                throw new ArgumentNullException(nameof(metaKey));
            }
            _fieldMap.Add(fieldName, metaKey);
            return this;
        }


        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.WriteStartArray();

                foreach (var doc in inputs)
                {
                    writer.WriteStartObject();

                        writer.WritePropertyName("type");
                        writer.WriteValue("add");

                        writer.WritePropertyName("id");
                        writer.WriteValue(
                            _idMetaKey != null ? doc.String(_idMetaKey) : doc.Id
                            );
                                          
                        writer.WritePropertyName("fields");
                        writer.WriteStartObject();

                            if (_bodyField != null)
                            {
                                writer.WritePropertyName(_bodyField);
                                writer.WriteValue(doc.Content);
                            }

                            foreach(var field in _fieldMap)
                            {
                                writer.WritePropertyName(field.Key);
                                writer.WriteValue(doc.String(field.Value));
                            }

                        writer.WriteEndObject();

                    writer.WriteEndObject();
                }

                writer.WriteEndArray();

                return new[] { context.GetNewDocument(sw.ToString()) };
            }
        }
    }
}
