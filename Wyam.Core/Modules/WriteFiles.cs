using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core;
using Wyam.Core.Helpers;
using Wyam.Abstractions;

namespace Wyam.Core.Modules
{
    public class WriteFiles : IModule
    {
        private readonly Func<IDocument, string> _path;
        private Func<IDocument, bool> _where = null; 

        public WriteFiles(Func<IDocument, string> path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            _path = path;
        }

        public WriteFiles(string extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException("extension");
            }

            _path = x =>
            {
                string fileRelative = x.Get<string>(MetadataKeys.RelativeFilePath, null);
                if (!string.IsNullOrWhiteSpace(fileRelative))
                {
                    return Path.ChangeExtension(fileRelative, extension);
                }
                return null;
            };
        }

        public WriteFiles()
        {
            _path = m => (string)m.Metadata.Get(MetadataKeys.RelativeFilePath);
        }

        public WriteFiles Where(Func<IDocument, bool> predicate)
        {
            _where = predicate;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            foreach (IDocument input in inputs.Where(x => _where == null || _where(x)))
            {
                bool wrote = false;
                string path = input.Get<string>(MetadataKeys.WritePath, null);
                if (path == null && input.Metadata.ContainsKey(MetadataKeys.WriteExtension)
                    && input.Metadata.ContainsKey(MetadataKeys.RelativeFilePath))
                {
                    path = Path.ChangeExtension(input.Get<string>(MetadataKeys.RelativeFilePath), input.Get<string>(MetadataKeys.WriteExtension));
                }
                if (path == null)
                {
                    path = _path(input);
                }
                if (path != null)
                {
                    path = Path.Combine(context.OutputFolder, path);
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        string pathDirectory = Path.GetDirectoryName(path);
                        if (!Directory.Exists(pathDirectory))
                        {
                            Directory.CreateDirectory(pathDirectory);
                        }
                        File.WriteAllText(path, input.Content);
                        context.Trace.Verbose("Wrote file {0}", path);
                        wrote = true;
                        yield return input.Clone(new Dictionary<string, object>
                        {
                            { MetadataKeys.DestinationFilePath, path }
                        });
                    }
                }
                if (!wrote)
                {
                    yield return input;
                }
            }
        }
    }
}
