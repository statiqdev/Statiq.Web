using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Modules.Razor
{
    // From https://github.com/aspnet/Mvc/blob/dev/src/Microsoft.AspNet.Mvc.Razor/HelperResult.cs
    public class HelperResult
    {
        private readonly Action<TextWriter> _action;

        /// <summary>
        /// Creates a new instance of <see cref="HelperResult"/>.
        /// </summary>
        /// <param name="action">The delegate to invoke when <see cref="WriteTo(TextWriter)"/> is called.</param>
        public HelperResult(Action<TextWriter> action)
        {
            _action = action;
        }

        /// <summary>
        /// Gets the delegate to invoke when <see cref="WriteTo(TextWriter)"/> is called.
        /// </summary>
        public Action<TextWriter> WriteAction
        {
            get { return _action; }
        }

        /// <summary>
        /// Method invoked to produce content from the <see cref="HelperResult"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        public virtual void WriteTo(TextWriter writer)
        {
            _action(writer);
        }
    }
}
