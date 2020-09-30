using System;
using Microsoft.Extensions.DependencyInjection;
using Statiq.App;
using Statiq.Common;

namespace Statiq.Web
{
    public static class BootstrapperThemeExtensions
    {
        public static Bootstrapper ConfigureThemePaths(this Bootstrapper bootstrapper, Action<PathCollection> action) =>
            bootstrapper.ConfigureServices(services =>
                action(services
                    .BuildServiceProvider() // We need to build an intermediate service provider to get access to the singleton
                    .GetRequiredService<ThemeManager>()
                    .ThemePaths));

        public static Bootstrapper AddThemePath(this Bootstrapper bootstrapper, NormalizedPath themePath) =>
            bootstrapper.ConfigureThemePaths(paths => paths.Add(themePath));

        public static Bootstrapper SetThemePath(this Bootstrapper bootstrapper, NormalizedPath themePath) =>
            bootstrapper.ConfigureThemePaths(paths =>
            {
                paths.Clear();
                paths.Add(themePath);
            });
    }
}
