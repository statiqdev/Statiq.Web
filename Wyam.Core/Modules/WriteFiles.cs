using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core;
using Wyam.Common;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;
using Wyam.Core.Documents;

namespace Wyam.Core.Modules
{
    public class WriteFiles : IModule
    {
        private readonly DocumentConfig _path;
        private Func<IDocument, IExecutionContext, bool> _predicate = null; 

        // The predicate should return a string
        public WriteFiles(DocumentConfig path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            _path = path;
        }

        public WriteFiles(string extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            _path = (x, y) =>
            {
                string fileRelative = x.String(MetadataKeys.RelativeFilePath);
                if (!string.IsNullOrWhiteSpace(fileRelative))
                {
                    return Path.ChangeExtension(fileRelative, extension);
                }
                return null;
            };
        }

        public WriteFiles()
        {
            _path = (x, y) => x.String(MetadataKeys.RelativeFilePath);
        }

        // The delegate should return a bool
        public WriteFiles Where(DocumentConfig predicate)
        {
            Func<IDocument, IExecutionContext, bool> currentPredicate = _predicate;
            _predicate = currentPredicate == null
                ? (Func<IDocument, IExecutionContext, bool>)(predicate.Invoke<bool>)
                : ((x, c) => currentPredicate(x, c) && predicate.Invoke<bool>(x, c));
            return this;
        }

        protected bool ShouldProcess(IDocument input, IExecutionContext context)
        {
            return _predicate == null || _predicate(input, context);
        }

        protected string GetPath(IDocument input, IExecutionContext context)
        {
            // WritePath
            string path = input.String(MetadataKeys.WritePath);
            if (!string.IsNullOrWhiteSpace(path))
            {
                path = PathHelper.NormalizePath(path);
            }

            // WriteFileName
            if (string.IsNullOrWhiteSpace(path) && input.ContainsKey(MetadataKeys.WriteFileName)
                && input.ContainsKey(MetadataKeys.RelativeFileDir))
            {
                path = Path.Combine(input.String(MetadataKeys.RelativeFileDir),
                    PathHelper.NormalizePath(input.String(MetadataKeys.WriteFileName)));
            }

            // WriteExtension
            if (string.IsNullOrWhiteSpace(path) && input.ContainsKey(MetadataKeys.WriteExtension)
                && input.ContainsKey(MetadataKeys.RelativeFilePath))
            {
                path = Path.ChangeExtension(input.String(MetadataKeys.RelativeFilePath),
                    input.String(MetadataKeys.WriteExtension));
            }

            // Func
            if (string.IsNullOrWhiteSpace(path))
            {
                path = _path.Invoke<string>(input, context);
            }

            if (!string.IsNullOrWhiteSpace(path))
            {
                path = Path.GetFullPath(Path.Combine(context.OutputFolder, path));
                if (!string.IsNullOrWhiteSpace(path))
                {
                    return path;
                }
            }

            return null;
        }

        public virtual IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.AsParallel().Select(input =>
            {
                if (ShouldProcess(input, context))
                {
                    string path = GetPath(input, context);
                    if (path != null)
                    {
                        string pathDirectory = Path.GetDirectoryName(path);
                        if (pathDirectory != null && !Directory.Exists(pathDirectory))
                        {
                            Directory.CreateDirectory(pathDirectory);
                        }
                        FileStream outputStream = File.Open(path, FileMode.Create);
                        using (Stream inputStream = input.GetStream())
                        {
                            inputStream.CopyTo(outputStream);
                        }
                        context.Trace.Verbose("Wrote file {0}", path);
                        return input.Clone(outputStream, new Dictionary<string, object>
                        {
                            {MetadataKeys.DestinationFileBase, Path.GetFileNameWithoutExtension(path)},
                            {MetadataKeys.DestinationFileExt, Path.GetExtension(path)},
                            {MetadataKeys.DestinationFileName, Path.GetFileName(path)},
                            {MetadataKeys.DestinationFileDir, Path.GetDirectoryName(path)},
                            {MetadataKeys.DestinationFilePath, path},
                            {MetadataKeys.DestinationFilePathBase, PathHelper.RemoveExtension(path)}
                        });
                    }
                }
                return input;
            });
        }
    }
}
