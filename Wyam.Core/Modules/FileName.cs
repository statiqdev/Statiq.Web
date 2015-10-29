using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;
using Wyam.Core.Documents;
using Metadata = Wyam.Common.Documents.Metadata;

namespace Wyam.Core.Modules
{
    public class FileName : IModule
    {
        internal static readonly string[] ReservedChars = new string[]
        {
            "-", ".", "_", "~", ":", "/", "?", "#", "[", "]",
            "@", "!", "$", "&", "'", "(", ")", "*", "+", ",",
            ";", "=", "}", ";"
        };

        private static readonly Regex FileNameRegex = new Regex("^([a-zA-Z0-9])+$");

        private readonly DocumentConfig _fileName = (d, c) => d.String(MetadataKeys.SourceFileName);
        private readonly string _outputKey = MetadataKeys.WriteFileName;
        private string _pathOutputKey = MetadataKeys.WritePath;  // null for no output path

        // Sets metadata key WriteFileName to optimized version of SourceFileName
        // Also sets metadata key WritePath to Path.Combine(RelativeFileDir, WriteFileName)
        public FileName()
        {
        }

        // Sets metadata key WriteFileName to optimized version of specified inputKey
        // Also sets metadata key WritePath to Path.Combine(RelativeFileDir, WriteFileName)
        public FileName(string inputKey)
        {
            if (inputKey == null)
            {
                throw new ArgumentNullException(nameof(inputKey));
            }
            if (string.IsNullOrWhiteSpace(inputKey))
            {
                throw new ArgumentException(nameof(inputKey));
            }
            _fileName = (d, c) => d.String(inputKey);
        }

        public FileName(DocumentConfig fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }
            _fileName = fileName;
        }

        // Sets metadata for the specified outputKey to optimized version of specified inputKey
        // Does not automatically set WritePath
        public FileName(string inputKey, string outputKey)
        {
            if (inputKey == null)
            {
                throw new ArgumentNullException(nameof(inputKey));
            }
            if (string.IsNullOrWhiteSpace(inputKey))
            {
                throw new ArgumentException(nameof(inputKey));
            }
            if (outputKey == null)
            {
                throw new ArgumentNullException(nameof(outputKey));
            }
            if (string.IsNullOrWhiteSpace(outputKey))
            {
                throw new ArgumentException(nameof(outputKey));
            }
            _fileName = (d, c) => d.String(inputKey);
            _outputKey = outputKey;
        }
        
        public FileName(DocumentConfig fileName, string outputKey)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }
            if (outputKey == null)
            {
                throw new ArgumentNullException(nameof(outputKey));
            }
            if (string.IsNullOrWhiteSpace(outputKey))
            {
                throw new ArgumentException(nameof(outputKey));
            }
            _fileName = fileName;
            _outputKey = outputKey;
        }

        // Sets metadata key WritePath to Path.Combine(RelativeFileDir, WriteFileName)
        public FileName PreservePath(bool preservePath)
        {
            if (!preservePath)
            {
                _pathOutputKey = null;
            }
            return this;
        }

        // Sets metadata for the specified outputKey to Path.Combine(RelativeFileDir, WriteFileName)
        public FileName PreservePath(string outputKey)
        {
            if (outputKey == null)
            {
                throw new ArgumentNullException(nameof(outputKey));
            }
            if (string.IsNullOrWhiteSpace(outputKey))
            {
                throw new ArgumentException(nameof(outputKey));
            }
            _pathOutputKey = outputKey;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.AsParallel().Select(input =>
            {
                string fileName = _fileName.Invoke<string>(input, context);
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = GetFileName(fileName);
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        string relativeFileDir = input.String(MetadataKeys.RelativeFileDir);
                        if (!string.IsNullOrWhiteSpace(_pathOutputKey) && !string.IsNullOrWhiteSpace(relativeFileDir))
                        {
                            return input.Clone(new[]
                            {
                                Metadata.Create(_outputKey, fileName),
                                Metadata.Create(_pathOutputKey, Path.Combine(relativeFileDir, fileName))
                            });
                        }
                        return input.Clone(new[] {Metadata.Create(_outputKey, fileName)});
                    }
                }
                return input;
            });
        }

        private static string GetFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return string.Empty;
            }

            // Remove multiple dashes
            fileName = Regex.Replace(fileName, @"\-{2,}", "");

            // Remove reserved chars - doing this as an array reads a lot better than a regex
            foreach (string token in ReservedChars)
            {
                fileName = fileName.Replace(token, "");
            }

            // Remove multiple spaces
            fileName = Regex.Replace(fileName, @"\s+", " ");

            // Turn spaces into dashes
            fileName = fileName.Replace(" ", "-");

            // Grab letters and numbers only, use a regex to be unicode-friendly
            if (FileNameRegex.IsMatch(fileName))
            {
                fileName = FileNameRegex.Matches(fileName)[0].Value;
            }

            return fileName;
        }
    }
}
