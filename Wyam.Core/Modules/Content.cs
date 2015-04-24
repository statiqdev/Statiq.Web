using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core.Modules
{
    // Overwrites the existing content with the specified content
    public class Content : IModule
    {
        private readonly string _content;

        public Content(string content)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            _content = content;
        }

        public IEnumerable<IModuleContext> Execute(IEnumerable<IModuleContext> inputs, IPipelineContext pipeline)
        {
            return inputs.Select(x => x.Clone(_content));
        }
    }
}
