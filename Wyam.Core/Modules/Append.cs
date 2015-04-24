using System;
using System.Collections.Generic;
using System.Linq;

namespace Wyam.Core.Modules
{
    // Appends the specified content to the existing content
    public class Append : IModule
    {
        private readonly string _content;

        public Append(string content)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            _content = content;
        }

        public IEnumerable<IModuleContext> Execute(IEnumerable<IModuleContext> inputs, IPipelineContext pipeline)
        {
            return inputs.Select(x => x.Clone(x.Content + _content));
        }
    }
}