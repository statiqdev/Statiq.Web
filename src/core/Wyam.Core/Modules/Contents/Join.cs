using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;

namespace Wyam.Core.Modules.Contents
{
    /// <summary>
    /// Joins documents together with an optional delimiter to form one document.
    /// </summary>
    /// <category>Content</category>
    public class Join : IModule
    {
        private readonly string _delimiter;
        private readonly JoinedMetadata _metaDataMode;

        /// <summary>
        /// Concatenates multiple documents together to form a single document without a delimiter and with the default metadata only
        /// </summary>
        public Join()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Concatenates multiple documents together to form a single document without a delimiter using the specified meta data mode
        /// </summary>
        /// <param name="metaDataMode">The specified metadata mode</param>
        public Join(JoinedMetadata metaDataMode)
            : this(string.Empty, metaDataMode)
        {
        }

        /// <summary>
        /// Concatenates multiple documents together to form a single document with a specified delimiter using the specified meta data mode
        /// </summary>
        /// <param name="delimiter">The string to use as a separator between documents</param>
        /// <param name="metaDataMode">The specified metadata mode</param>
        public Join(string delimiter, JoinedMetadata metaDataMode = JoinedMetadata.DefaultOnly)
        {
            _delimiter = delimiter;
            _metaDataMode = metaDataMode;
        }

        /// <summary>
        /// Returns a single document containing the concatenated content of all input documents with an optional delimiter and configurable metadata options
        /// </summary>
        /// <returns>A single document in a list</returns>
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            StringBuilder contentBuilder = new StringBuilder();

            if (inputs == null || inputs.Count < 1)
            {
                return new List<IDocument>() { context.GetDocument() };
            }

            foreach (var document in inputs)
            {
                if (document == null)
                {
                    continue;
                }

                contentBuilder.Append(document.Content);
                contentBuilder.Append(_delimiter);
            }

            contentBuilder.Remove(contentBuilder.Length - _delimiter.Length, _delimiter.Length);

            return new List<IDocument>() { context.GetDocument(contentBuilder.ToString(), MetadataForOutputDocument(inputs)) };
        }

        /// <summary>
        /// returns the correct metadata for the new document based on the provided list of documents and the selected metadata mode
        /// </summary>
        /// <param name="inputs">The list of input documents</param>
        private IEnumerable<KeyValuePair<string, object>> MetadataForOutputDocument(IReadOnlyList<IDocument> inputs)
        {
            switch (_metaDataMode)
            {
                case JoinedMetadata.FirstDocument:
                    return inputs.First().Metadata.ToList();

                case JoinedMetadata.LastDocument:
                    return inputs.Last().Metadata.ToList();

                case JoinedMetadata.AllWithFirstDuplicates:
                    {
                        return inputs.SelectMany(a => a.Metadata).GroupBy(b => b.Key).ToDictionary(g => g.Key, g => g.First().Value).ToArray();
                    }

                case JoinedMetadata.AllWithLastDuplicates:
                    {
                        return inputs.SelectMany(a => a.Metadata).GroupBy(b => b.Key).ToDictionary(g => g.Key, g => g.Last().Value).ToArray();
                    }
                case JoinedMetadata.DefaultOnly:
                    return new List<KeyValuePair<string, object>>();

                default:
                    throw new ArgumentOutOfRangeException($"{nameof(JoinedMetadata)} option was not expected.");
            }
        }
    }
}
