using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core;
using Wyam.Core.Helpers;

namespace Wyam.Core.Modules
{
    public class WriteFiles : IModule
    {
        private readonly Func<IMetadata, string> _path;

        public WriteFiles(Func<IMetadata, string> path)
        {
            if (path == null) throw new ArgumentNullException("path");

            _path = path;
        }

        public IEnumerable<IPipelineContext> Prepare(IPipelineContext context)
        {
            string path = _path(context.Metadata);
            if (path == null)
            {
                yield break;
            }
            path = Path.Combine(Environment.CurrentDirectory, path);
            yield return context.Clone(path);
        }

        public string Execute(IPipelineContext context, string content)
        {
            if (context.PersistedObject != null)
            {
                string path = (string) context.PersistedObject;
                if (!string.IsNullOrWhiteSpace(path))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllText(path, content);
                }
            }
            return content;
        }
    }
}
