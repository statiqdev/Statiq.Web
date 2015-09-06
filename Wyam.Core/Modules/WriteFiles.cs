using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core;
using Wyam.Common;
using Wyam.Core.Documents;

namespace Wyam.Core.Modules
{
    public class WriteFiles : IModule
    {
        private readonly DocumentConfig _path;
        private DocumentConfig _where = null; 

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
            _where = predicate;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.AsParallel().Select(input =>
            {
                if (_where == null || _where.Invoke<bool>(input, context))
                {
                    // WritePath
                    string path = input.String(MetadataKeys.WritePath);
                    if (path != null)
                    {
                        path = PathHelper.NormalizePath(path);
                    }

                    // WriteFileName
                    if (path == null && input.ContainsKey(MetadataKeys.WriteFileName)
                        && input.ContainsKey(MetadataKeys.RelativeFileDir))
                    {
                        path = Path.Combine(input.String(MetadataKeys.RelativeFileDir),
                            PathHelper.NormalizePath(input.String(MetadataKeys.WriteFileName)));
                    }

                    // WriteExtension
                    if (path == null && input.ContainsKey(MetadataKeys.WriteExtension)
                        && input.ContainsKey(MetadataKeys.RelativeFilePath))
                    {
                        path = Path.ChangeExtension(input.String(MetadataKeys.RelativeFilePath),
                            input.String(MetadataKeys.WriteExtension));
                    }

                    // Func
                    if (path == null)
                    {
                        path = _path.Invoke<string>(input, context);
                    }

                    if (path != null)
                    {
                        path = Path.GetFullPath(Path.Combine(context.OutputFolder, path));
                        if (!string.IsNullOrWhiteSpace(path))
                        {
                            string pathDirectory = Path.GetDirectoryName(path);
                            if (!Directory.Exists(pathDirectory))
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
                }
                return input;
            });
        }
    }
}
