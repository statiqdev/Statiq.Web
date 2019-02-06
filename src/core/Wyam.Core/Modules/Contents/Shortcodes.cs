using System;
using System.Collections.Generic;
using System.Text;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Core.Shortcodes;

namespace Wyam.Core.Modules.Contents
{
    public class Shortcodes : IModule
    {
        private readonly string _startDelimiter = ShortcodeParser.DefaultStartDelimiter;
        private readonly string _endDelimitr = ShortcodeParser.DefaultEndDelimiter;

        public Shortcodes()
        {
        }

        public Shortcodes(string startDelimiter, string endDelimiter)
        {
            _startDelimiter = startDelimiter;
            _endDelimitr = endDelimiter;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
