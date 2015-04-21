using System;
using System.Collections.Generic;

namespace Wyam.Core.Modules
{
    public class Append : Module
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

        internal protected override IEnumerable<IPipelineContext> Prepare(IPipelineContext context)
        {
            return new[] { context };
        }

        internal protected override string Execute(IPipelineContext context, string content)
        {
            return content + _content;
        }
    }
}