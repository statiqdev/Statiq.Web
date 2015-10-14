using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Pipelines;
using Wyam.Core.Documents;

namespace Wyam.Core.Modules
{
    public class UnwrittenFiles : WriteFiles
    {
        public UnwrittenFiles(DocumentConfig path) : base(path)
        {
        }

        public UnwrittenFiles(string extension) : base(extension)
        {
        }

        public UnwrittenFiles()
        {
        }

        public override IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.AsParallel().Select(input =>
            {
                if (ShouldProcess(input, context))
                {
                    string path = GetPath(input, context);
                    if (path != null)
                    {
                        string pathDirectory = Path.GetDirectoryName(path);
                        if ((pathDirectory != null && Directory.Exists(pathDirectory) && File.Exists(path)) || (pathDirectory == null && File.Exists(path)))
                        {
                            return null;
                        }
                    }
                }
                return input;
            }).Where(x => x != null);
        }
    }
}
