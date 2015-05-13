using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Modules.Razor
{
    // From https://github.com/aspnet/Mvc/blob/dev/src/Microsoft.AspNet.Mvc.Razor/RenderAsyncDelegate.cs
    public delegate Task RenderAsyncDelegate(TextWriter writer);
}
