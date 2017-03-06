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
    /// Joins documents together with an optional deliminator to form one document
    /// </summary>
    /// <category>Content</category>
    public class Join : IModule
    {
        public enum JoinMetaDataOptions
        {
            DefaultOnly,
            FirstDocument,
            LastDocument,
            All_KeepFirstRepeats,
            All_KeepLastRepeats
        }


        private readonly string _delimiter;
        private readonly JoinMetaDataOptions _metaDataMode;

        /// <summary>
        /// Concatenates multiple documents together to form a single document without a delimiter and with the default metadata only
        /// </summary>        
        public Join() : this("")
        {
            
        }

        /// <summary>
        /// Concatenates multiple documents together to form a single document without a delimiter using the specified meta data mode
        /// </summary>
        /// <param name="metaDataMode">The specified metaData mode</param>
        public Join(JoinMetaDataOptions metaDataMode) : this("", metaDataMode)
        {

        }

        /// <summary>
        /// Concatanetes multiple documents together to form a single document with a specified delimiter and with the default metadata only
        /// </summary>
        /// <param name="delimiter">The string to use as a seperator between documents</param>
        public Join(string delimiter) : this(delimiter, JoinMetaDataOptions.DefaultOnly)
        {
                        
        }

        /// <summary>
        /// Concatenates multiple documents together to form a single document with a specified delimiter using the specified meta data mode
        /// </summary>
        /// <param name="delimiter">The string to use as a seperator between documents</param>
        /// <param name="metaDataMode">The specified metaData mode</param>
        public Join(string delimiter, JoinMetaDataOptions metaDataMode)
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

            foreach(var document in inputs)
            {
                if (document == null) continue;

                contentBuilder.Append(document.Content);
                contentBuilder.Append(_delimiter);
            }

            contentBuilder.Remove(contentBuilder.Length - _delimiter.Length, _delimiter.Length);
            
            return new List<IDocument>() { context.GetDocument(contentBuilder.ToString(), MetaDataForOutputDocument(inputs)) };
        }

        /// <summary>
        /// returns the correct metadata for the new document based on the provided list of documents and the selected metadata mode
        /// </summary>
        /// <param name="inputs">The list of input documents</param>
        /// <returns></returns>
        private IEnumerable<KeyValuePair<string, object>> MetaDataForOutputDocument(IReadOnlyList<IDocument> inputs)
        {
            switch (_metaDataMode)
            {
                case JoinMetaDataOptions.FirstDocument:
                    return inputs.First().Metadata.ToList();

                case JoinMetaDataOptions.LastDocument:
                    return inputs.Last().Metadata.ToList();

                case JoinMetaDataOptions.All_KeepFirstRepeats:
                    {
                        return inputs.SelectMany(a => a.Metadata).GroupBy(b => b.Key).ToDictionary(g => g.Key, g => g.First().Value).ToArray();                        
                    }

                case JoinMetaDataOptions.All_KeepLastRepeats:
                    {
                        return inputs.SelectMany(a => a.Metadata).GroupBy(b => b.Key).ToDictionary(g => g.Key, g => g.Last().Value).ToArray();
                    }
                case JoinMetaDataOptions.DefaultOnly:
                    return new List<KeyValuePair<string, object>>();

                default:
                    throw new ArgumentOutOfRangeException("Join Metadata option was not expected.");
            }
        }
    }
}
