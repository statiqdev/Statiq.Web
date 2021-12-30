using Microsoft.Extensions.DependencyInjection;
using Statiq.Common;

namespace Statiq.Web
{
    /// <summary>
    /// Implement this interface to define an initializer that will get automatically
    /// instantiated and run by the theme manager.
    /// </summary>
    /// <remarks>
    /// In order for the theme manager to find the initializer, the class must be <c>public</c>.
    /// Note that settings from theme YAML or JSON configuration files are not yet added to the settings collection here.
    /// </remarks>
    public interface IThemeInitializer
    {
        void Initialize(ISettings settings, IServiceCollection serviceCollection, IReadOnlyFileSystem fileSystem);
    }
}