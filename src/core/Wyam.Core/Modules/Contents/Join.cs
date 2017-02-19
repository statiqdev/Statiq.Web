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

        /// <summary>
        /// Returns a single document containing the concatenated content of all input documents
        /// </summary>
        /// <returns>A single document in a list</returns>
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            StringBuilder contentBuilder = new StringBuilder();

            if (inputs == null)
            {
                return new List<IDocument>() { context.GetDocument() };
            }

            foreach(var document in inputs)
            {
                if (document == null) continue;

                contentBuilder.Append(document.Content);
            }
            
            return new List<IDocument>() { context.GetDocument(contentBuilder.ToString(), new KeyValuePair<string, object>[0]) };
        }
    }

    //TODO - update comments - meta data and delimator
}
