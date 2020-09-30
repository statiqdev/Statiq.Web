using Statiq.App;

namespace Statiq.Web
{
    public static class BootstrapperFactoryExtensions
    {
        /// <summary>
        /// Creates a bootstrapper with all functionality for Statiq Web.
        /// </summary>
        /// <param name="factory">The bootstrapper factory.</param>
        /// <param name="args">The command line arguments.</param>
        /// <returns>A bootstrapper.</returns>
        public static Bootstrapper CreateWeb(this BootstrapperFactory factory, string[] args) =>
            factory.CreateDefault(args).AddWeb();
    }
}
