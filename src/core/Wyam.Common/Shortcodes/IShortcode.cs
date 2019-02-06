using System;
using System.Text;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;

namespace Wyam.Common.Shortcodes
{
    /// <summary>
    /// Contains the code for a given shortcode (see the <c>Shortcodes</c> module).
    /// </summary>
    public interface IShortcode
    {
        IShortcodeResult Execute(string[] args, string content, IMetadata metadata, IExecutionContext context);
    }
}
