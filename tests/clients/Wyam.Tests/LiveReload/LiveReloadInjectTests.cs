using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.Testing;

using NUnit.Framework;

using Owin;

using Wyam.LiveReload;

namespace Wyam.Tests.LiveReload
{
    [TestFixture]
    public class LiveReloadInjectTests
    {
        private static readonly Assembly ContentAssembly = typeof(LiveReloadInjectTests).Assembly;
        private static readonly string ContentNamespace = $"{typeof(LiveReloadInjectTests).Namespace}.Documents";

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly LiveReloadServer _server;
        private readonly TestServer _host;

        public LiveReloadInjectTests()
        {
            _server = new LiveReloadServer();

            _host = TestServer.Create(app =>
            {
                _server.AddInjectionMiddleware(app);
                IFileSystem reloadFilesystem = new EmbeddedResourceFileSystem(ContentAssembly, ContentNamespace);
                app.UseStaticFiles(new StaticFileOptions
                {
                    RequestPath = PathString.Empty,
                    FileSystem = reloadFilesystem,
                    ServeUnknownFileTypes = true
                });
                _server.AddHostMiddleware(app);
            });
        }

        [Test]
        public async Task WhenServingHtmlInjectScript()
        {
            const string filename = "BasicHtmlDocument.html";
            HttpResponseMessage response = await _host.CreateRequest(filename).GetAsync();
            string body = await response.Content.ReadAsStringAsync();

            Assert.IsTrue(body.Contains("<script type=\"text/javascript\" src=\"/livereload.js\">"));
        }

        [Test]
        public async Task WhenServingNonHtmlDoNotModify()
        {
            const string filename = "NonHtmlDocument.css";
            HttpResponseMessage response = await _host.CreateRequest(filename).GetAsync();
            string body = await response.Content.ReadAsStringAsync();

            string expected = ReadFile(filename);
            Assert.AreEqual(expected, body);
        }

        private string ReadFile(string filename)
        {
            string resourceName = $"{ContentNamespace}.{filename}";
            using (Stream stream = ContentAssembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    return null;
                }
                StreamReader reader = new StreamReader(stream);
                string fileContent = reader.ReadToEnd();
                return fileContent;
            }
        }
    }
}