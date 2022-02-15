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
    /// <category name="Content" />
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
            Stream contentStream = context.GetContentStream();
            using (TextWriter textWriter = new StreamWriter(contentStream))
            {
                using (JsonWriter writer = new JsonTextWriter(textWriter))
                {
                    await writer.WriteStartArrayAsync();

                    foreach (IDocument input in context.Inputs)
                    {
                        await writer.WriteStartObjectAsync();

                        await writer.WritePropertyNameAsync("type");
                        await writer.WriteValueAsync("add");

                        await writer.WritePropertyNameAsync("id");
                        await writer.WriteValueAsync(_idMetaKey is object ? input.GetString(_idMetaKey) : input.Id.ToString());

                        await writer.WritePropertyNameAsync("fields");
                        await writer.WriteStartObjectAsync();

                        if (_bodyField is object)
                        {
                            await writer.WritePropertyNameAsync(_bodyField);
                            await writer.WriteValueAsync(await input.GetContentStringAsync());
                        }

                        foreach (Field field in _fields)
                        {
                            string name = await field.FieldName.GetValueAsync(input, context);
                            object value = await field.FieldValue.GetValueAsync(input, context);
                            if (name is null || value is null)
                            {
                                // Null fields are not written
                                continue;
                            }

                            await writer.WritePropertyNameAsync(name);
                            await writer.WriteRawValueAsync(JsonConvert.SerializeObject(value));
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
                            if (value is null || fieldName is null)
                            {
                                // Null fields are not written
                                continue;
                            }

                            if (field.Transformer is object)
                            {
                                value = field.Transformer.Invoke(value);
                            }
                            if (value is null)
                            {
                                // If the transformer function returns null, we'll not writing this either
                                continue;
                            }

                            await writer.WritePropertyNameAsync(fieldName);
                            await writer.WriteRawValueAsync(JsonConvert.SerializeObject(value));
                        }

                        await writer.WriteEndObjectAsync();

                        await writer.WriteEndObjectAsync();
                    }

                    await writer.WriteEndArrayAsync();
                    await textWriter.FlushAsync();

                    return context.CreateDocument(context.GetContentProvider(contentStream, MediaTypes.Json)).Yield();
                }
            }
        }
    }
}