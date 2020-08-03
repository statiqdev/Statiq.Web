using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Statiq.Common;

namespace Statiq.Web.Aws
{
    /// <summary>
    /// Generates bulk JSON upload data to get documents into Amazon CloudSearch.
    /// </summary>
    /// <remarks>
    /// This module creates a single document from a pipeline with JSON data containing the correctly formatted commands for Amazon CloudSearch.
    /// Note that this just creates the document. Once that document is written to the file system, you will still need to upload the document
    /// to a correctly configured CloudSearch instance using the API or Amazon CLI.
    /// </remarks>
    /// <category>Content</category>
    public class GenerateCloudSearchData : Module
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
        /// <param name="transformer">If specified, it will be invoked on the meta value prior to serialization. If the function returns NULL, the field will not be written.</param>
        /// <returns>The current module.</returns>
        public GenerateCloudSearchData MapMetaField(Config<string> fieldName, Config<string> metaKey, Func<object, object> transformer = null)
        {
            fieldName.ThrowIfNull(nameof(fieldName));
            metaKey.ThrowIfNull(nameof(metaKey));
            _metaFields.Add(new MetaFieldMapping(fieldName, metaKey, transformer));
            return this;
        }

        /// <summary>
        /// Adds a field value.
        /// </summary>
        /// <param name="fieldName">The CloudSearch field name.</param>
        /// <param name="fieldValue">The value.</param>
        /// <returns>The current module.</returns>
        public GenerateCloudSearchData AddField(Config<string> fieldName, Config<object> fieldValue)
        {
            _fields.Add(new Field(fieldName, fieldValue));
            return this;
        }

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            Stream contentStream = await context.GetContentStreamAsync();
            using (TextWriter textWriter = new StreamWriter(contentStream))
            {
                using (JsonWriter writer = new JsonTextWriter(textWriter))
                {
                    writer.WriteStartArray();

                    foreach (IDocument input in context.Inputs)
                    {
                        writer.WriteStartObject();

                        writer.WritePropertyName("type");
                        writer.WriteValue("add");

                        writer.WritePropertyName("id");
                        writer.WriteValue(_idMetaKey != null ? input.GetString(_idMetaKey) : input.Id.ToString());

                        writer.WritePropertyName("fields");
                        writer.WriteStartObject();

                        if (_bodyField != null)
                        {
                            writer.WritePropertyName(_bodyField);
                            writer.WriteValue(await input.GetContentStringAsync());
                        }

                        foreach (Field field in _fields)
                        {
                            string name = await field.FieldName.GetValueAsync(input, context);
                            object value = await field.FieldValue.GetValueAsync(input, context);
                            if (name == null || value == null)
                            {
                                // Null fields are not written
                                continue;
                            }

                            writer.WritePropertyName(name);
                            writer.WriteRawValue(JsonConvert.SerializeObject(value));
                        }

                        foreach (MetaFieldMapping field in _metaFields)
                        {
                            string metaKey = await field.MetaKey.GetValueAsync(input, context);
                            string fieldName = await field.FieldName.GetValueAsync(input, context);

                            if (!input.ContainsKey(metaKey))
                            {
                                continue;
                            }

                            object value = input.Get(metaKey);
                            if (value == null || fieldName == null)
                            {
                                // Null fields are not written
                                continue;
                            }

                            if (field.Transformer != null)
                            {
                                value = field.Transformer.Invoke(value);
                            }
                            if (value == null)
                            {
                                // If the transformer function returns null, we'll not writing this either
                                continue;
                            }

                            writer.WritePropertyName(fieldName);
                            writer.WriteRawValue(JsonConvert.SerializeObject(value));
                        }

                        writer.WriteEndObject();

                        writer.WriteEndObject();
                    }

                    writer.WriteEndArray();
                    textWriter.Flush();

                    return context.CreateDocument(context.GetContentProvider(contentStream, MediaTypes.Json)).Yield();
                }
            }
        }
    }
}
