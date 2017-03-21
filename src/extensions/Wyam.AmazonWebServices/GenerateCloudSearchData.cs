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
using Wyam.Common.Execution;
using Wyam.Common.Util;

namespace Wyam.AmazonWebServices
{
    /// <summary>
    /// Generates bulk JSON upload data to get documents into Amazon CloudSearch.
    /// </summary>
    /// <remarks>
    /// This module creates a single document from a pipeline with JSON data containing the correctly formatted commands for Amazon CloudSearch.
    /// Note that this just creates the document. Once that document is written to the file system, you will still need to upload the document
    /// to a correctly configured CloudSearch instance using the API or Amazon CLI.
    /// </remarks>
    /// <example>
    /// <code>
    /// Pipelines.Add("CloudSearchData",
    ///     Documents("NameOfAPriorPipeline"),
    ///     GenerateCloudSearchData("Id", "Body")
    ///        .AddField("type", "post")
    ///        .AddField("length", d => d.Content.Count())
    ///        .MapMetaField("title", "Title")
    ///        .MapMetaField("tags", "Tags", o => o.Split(",".ToCharArray())),
    ///     Meta("WritePath", "cloudsearch_data.json"),
    ///     WriteFiles()
    /// );
    /// </code>
    /// </example>
    /// <category>Content</category>
    public class GenerateCloudSearchData : IModule
    {
        private readonly string _idMetaKey;
        private readonly string _bodyField;
        private readonly List<MetaFieldMapping> _metaFields;
        private readonly List<Field> _fields;


        /// <summary>
        /// Generates Amazon CloudSearch JSON data.
        /// </summary>
        /// <param name="idMetaKey">The meta key representing the unique ID for this document.  If NULL, the Document.Id will be used.</param>
        /// <param name="bodyField">The field name for the document contents.  If NULL, the document contents will not be written to the data.</param>
        public GenerateCloudSearchData(string idMetaKey, string bodyField)
        {
            _idMetaKey = idMetaKey;
            _bodyField = bodyField;
            _metaFields = new List<MetaFieldMapping>();
            _fields = new List<Field>();
        }

        /// <summary>
        /// Adds a mapping from meta key to CloudSearch field. When provided, the contents of the meta key will be written to the provided field name.
        /// </summary>
        /// <param name="fieldName">The CloudSearch field name.</param>
        /// <param name="metaKey">The meta key. If the meta key does not exist, the field will not be written.</param>
        /// <param name="transformer">An optional function that takes a string and returns an object. If specified, it will be invoked on the meta value prior to serialization. If the function returns NULL, the field will not be written.</param>
        /// <returns>The current module.</returns>
        public GenerateCloudSearchData MapMetaField(string fieldName, string metaKey, Func<object, object> transformer = null)
        {
            if (fieldName == null)
            {
                throw new ArgumentNullException(nameof(fieldName));
            }
            if (metaKey == null)
            {
                throw new ArgumentNullException(nameof(metaKey));
            }
            _metaFields.Add(new MetaFieldMapping(fieldName, metaKey, transformer));
            return this;
        }

        /// <summary>
        /// Adds a literal field value.
        /// </summary>
        /// <param name="fieldName">The CloudSearch field name.</param>
        /// <param name="fieldValue">The value.</param>
        /// <returns>The current module.</returns>
        public GenerateCloudSearchData AddField(string fieldName, object fieldValue)
        {
            _fields.Add(new Field(fieldName, fieldValue));
            return this;
        }

        /// <summary>
        /// Adds a function-based field value. The function will take in a document and return an object, which will be the field value.
        /// </summary>
        /// <param name="fieldName">The CloudSearch field name.</param>
        /// <param name="execute">A function of signature Func&lt;IDocument, object&gt;. If the function returns NULL, the field will not be written.</param>
        /// <returns>The current module.</returns>
        public GenerateCloudSearchData AddField(string fieldName, Func<IDocument, object> execute)
        {
            _fields.Add(new Field(fieldName, execute));
            return this;
        }

        /// <inheritdoc />
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            Stream contentStream = context.GetContentStream();
            using (TextWriter textWriter = new StreamWriter(contentStream))
            {
                using (JsonWriter writer = new JsonTextWriter(textWriter))
                {
                    writer.WriteStartArray();

                    context.ForEach(inputs, doc =>
                    {
                        writer.WriteStartObject();

                        writer.WritePropertyName("type");
                        writer.WriteValue("add");

                        writer.WritePropertyName("id");
                        writer.WriteValue(_idMetaKey != null ? doc.String(_idMetaKey) : doc.Id);

                        writer.WritePropertyName("fields");
                        writer.WriteStartObject();

                        if (_bodyField != null)
                        {
                            writer.WritePropertyName(_bodyField);
                            writer.WriteValue(doc.Content);
                        }

                        foreach (Field field in _fields)
                        {
                            object value = field.GetValue(doc);
                            if (value == null)
                            {
                                // Null fields are not written
                                continue;
                            }

                            writer.WritePropertyName(field.FieldName);
                            writer.WriteRawValue(JsonConvert.SerializeObject(value));
                        }

                        foreach (MetaFieldMapping field in _metaFields)
                        {
                            if (!doc.ContainsKey(field.MetaKey))
                            {
                                continue;
                            }

                            object value = doc.Get(field.MetaKey);
                            if (value == null)
                            {
                                // Null fields are not written
                                continue;
                            }

                            value = field.Transformer.Invoke(value.ToString());
                            if (value == null)
                            {
                                // If the transformer function returns null, we'll not writing this either
                                continue;
                            }

                            writer.WritePropertyName(field.FieldName);
                            writer.WriteRawValue(JsonConvert.SerializeObject(value));
                        }

                        writer.WriteEndObject();

                        writer.WriteEndObject();
                    });

                    writer.WriteEndArray();
                    textWriter.Flush();

                    return new[] { context.GetDocument(contentStream) };
                }
            }
        }
    }
}
