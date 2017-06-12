using System.Collections.Concurrent;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Wyam.Razor
{
    /// <summary>
    /// Razor compiler should be shared so that pages are only compiled once.
    /// </summary>
    internal class RazorService
    {
        private readonly ConcurrentDictionary<CompilationParameters, RazorCompiler> _compilers
            = new ConcurrentDictionary<CompilationParameters, RazorCompiler>();

        public void Render(RenderRequest request)
        {
            CompilationParameters parameters = new CompilationParameters
            {
                BasePageType = request.BaseType,
                DynamicAssemblies = new DynamicAssemblyCollection(request.Context.DynamicAssemblies),
                Namespaces = new NamespaceCollection(request.Context.Namespaces),
                FileSystem = request.Context.FileSystem
            };

            RazorCompiler compiler = _compilers.GetOrAdd(parameters, _ => new RazorCompiler(parameters));
            compiler.RenderPage(request);
        }

        public void ExpireChangeTokens()
        {
            foreach (RazorCompiler interpreter in _compilers.Values)
            {
                HostingEnvironment hostingEnviornment = (HostingEnvironment)interpreter.ServiceProvider.GetService<IHostingEnvironment>();
                hostingEnviornment.ExpireChangeTokens();
            }
        }
    }
}