using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Modules.Razor
{
    // From https://github.com/aspnet/Mvc/blob/dev/src/Microsoft.AspNet.Mvc.Core/ViewContext.cs
    public class ViewContext
    {
        public ViewContext(IView view, TextWriter writer)
        {
            View = view;
            Writer = writer;
        }

        public IView View { get; set; }

        public TextWriter Writer { get; set; }

        public string ExecutingFilePath { get; set; }
    }
}
