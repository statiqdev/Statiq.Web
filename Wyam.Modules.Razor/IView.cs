using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Modules.Razor
{
    // From https://github.com/aspnet/Mvc/blob/dev/src/Microsoft.AspNet.Mvc.Core/Rendering/ViewEngine/IView.cs
    public interface IView
    {
        string Path { get; }

        Task RenderAsync(ViewContext context);
    }
}
