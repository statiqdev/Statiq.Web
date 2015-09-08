using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common
{
    public interface IExecutionContext
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

        // This provides access to the same enhanced type conversion used to convert metadata types
        bool TryConvert<T>(object value, out T result);

        IDocument GetNewDocument(IEnumerable<KeyValuePair<string, object>> metadata = null);

        // This executes the specified modules with the specified input documents and returns the result documents
        IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs);

        // This executes the specified modules with an empty initial input document with optional additional metadata and returns the result documents
        IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<KeyValuePair<string, object>> metadata = null);
    }
}
