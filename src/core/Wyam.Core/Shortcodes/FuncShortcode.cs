using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Shortcodes;

namespace Wyam.Core.Shortcodes
{
    internal class FuncShortcode : IShortcode
    {
        private readonly Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, IShortcodeResult> _func;

        public FuncShortcode(Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, IShortcodeResult> func)
        {
            _func = func;
        }

        public IShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
            _func?.Invoke(args, content, document, context);
    }
}
