using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common
{
    public interface IExecutionContext : IMetadata
    {
        byte[] RawConfigAssembly { get; }
        IEnumerable<Assembly> Assemblies { get; } 
        IReadOnlyPipeline Pipeline { get; }
        IModule Module { get; }
        IExecutionCache ExecutionCache { get; }
        string RootFolder { get; }
        string InputFolder { get; }
        string OutputFolder { get; }
        ITrace Trace { get; }
        IDocumentCollection Documents { get; }
        IMetadata Metadata { get; }

        IExecutionContext Clone(IEnumerable<KeyValuePair<string, object>> metadata);

        // This executes the specified modules with the specified input documents and returns the result documents
        // If you pass in null for inputDocuments, a new input document with the initial metadata from the engine will be used
        IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs, IEnumerable<KeyValuePair<string, object>> metadata = null);
    }
}
