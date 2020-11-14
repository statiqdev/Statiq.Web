using System;
using Microsoft.Extensions.DependencyInjection;
using Statiq.App;
using Statiq.Common;

namespace Statiq.Web
{
    public static class BootstrapperThemeExtensions
    {
        public static TBootstrapper ConfigureThemePaths<TBootstrapper>(this TBootstrapper bootstrapper, Action<PathCollection> action)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureServices(services =>
                action(services
                    .BuildServiceProvider() // We need to build an intermediate service provider to get access to the singleton
                    .GetRequiredService<ThemeManager>()
                    .ThemePaths));

        public static TBootstrapper AddThemePath<TBootstrapper>(this TBootstrapper bootstrapper, NormalizedPath themePath)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureThemePaths(paths => paths.Add(themePath));

        public static TBootstrapper SetThemePath<TBootstrapper>(this TBootstrapper bootstrapper, NormalizedPath themePath)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureThemePaths(paths =>
            {
                paths.Clear();
                paths.Add(themePath);
            });
    }
}
