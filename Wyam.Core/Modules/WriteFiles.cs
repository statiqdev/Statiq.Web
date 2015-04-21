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
    public class WriteFiles : Module
    {
        private readonly Func<IMetadata, string> _path;

        public WriteFiles(Func<IMetadata, string> path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            _path = path;
        }

        public WriteFiles(string extension)
        {
            if (extension == null) throw new ArgumentNullException("extension");

            _path = m => (!m.ContainsKey("OutputPath") || !m.ContainsKey("FileRoot") || !m.ContainsKey("FileDir") || !m.ContainsKey("FileBase")) ? null :
                Path.Combine((string)m["OutputPath"], PathHelper.GetRelativePath((string)m["FileRoot"], (string)m["FileDir"]),
                    (string)m["FileBase"] + (extension.StartsWith(".") ? extension : ("." + extension)));
        }

        protected internal override IEnumerable<IPipelineContext> Prepare(IPipelineContext context)
        {
            string path = _path(context.Metadata);
            if (path == null)
            {
                yield break;
            }
            path = Path.Combine(Environment.CurrentDirectory, path);
            yield return context.Clone(path);
        }

        protected internal override string Execute(IPipelineContext context, string content)
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
