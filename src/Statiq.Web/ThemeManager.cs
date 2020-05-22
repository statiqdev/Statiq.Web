using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Statiq.App;
using Statiq.Common;
using Statiq.Web.Modules;

namespace Statiq.Web
{
    public class ThemeManager
    {
        public PathCollection ThemePaths { get; } = new PathCollection
        {
            "theme"
        };
    }
}
