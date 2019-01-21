using System;
using System.Collections.Generic;
using System.Text;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Shortcodes
{
    /// <summary>
    /// Contains the code for a given shortcode (see the <c>Shortcodes</c> module).
    /// </summary>
    public interface IShortcode
    {
        string Render(string[] args, string content, IDocument document, IExecutionContext context);
    }
}
