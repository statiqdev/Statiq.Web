using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core.Modules
{
    public class Prepend : Module
    {
        private readonly string _content;

        public Prepend(string content)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            _content = content;
        }

        protected internal override IEnumerable<IPipelineContext> Prepare(IPipelineContext context)
        {
            return new[] { context };
        }

        protected internal override string Execute(IPipelineContext context, string content)
        {
            return _content + content;
        }
    }
}
