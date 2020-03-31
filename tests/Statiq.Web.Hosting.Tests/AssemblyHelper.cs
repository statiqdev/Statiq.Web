using System.IO;
using System.Reflection;

namespace Statiq.Web.Hosting.Tests
{
    public static class AssemblyHelper
    {
        public static readonly Assembly TestAssembly = typeof(AssemblyHelper).Assembly;

        public static string ReadEmbeddedWebFile(string filename)
        {
            string resourceName = $"Statiq.Web.Hosting.Tests.wwwroot.{filename}";
            using (Stream stream = TestAssembly.GetManifestResourceStream(resourceName))
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
