using System;
using System.Collections.Generic;
using System.Text;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;

namespace Wyam.Core.Modules.Contents
{
    public class Shortcodes : IModule
    {
        private readonly bool _prerender;

        public Shortcodes(bool prerender)
        {
            _prerender = prerender;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
