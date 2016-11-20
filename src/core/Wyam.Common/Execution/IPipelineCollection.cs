using System.Collections;
using System.Collections.Generic;
using Wyam.Common.Modules;

namespace Wyam.Common.Execution
{
    public interface IPipelineCollection : IReadOnlyDictionary<string, IPipeline>, IReadOnlyList<IPipeline>
    {
        IPipeline Add(params IModule[] modules);
        IPipeline Add(string name, params IModule[] modules);
        IPipeline Add(bool processDocumentsOnce, params IModule[] modules);
        IPipeline Add(string name, bool processDocumentsOnce, params IModule[] modules);

        IPipeline InsertBefore(string target, params IModule[] modules);
        IPipeline InsertBefore(string target, string name, params IModule[] modules);
        IPipeline InsertBefore(string target, bool processDocumentsOnce, params IModule[] modules);
        IPipeline InsertBefore(string target, string name, bool processDocumentsOnce, params IModule[] modules);

        IPipeline InsertAfter(string target, params IModule[] modules);
        IPipeline InsertAfter(string target, string name, params IModule[] modules);
        IPipeline InsertAfter(string target, bool processDocumentsOnce, params IModule[] modules);
        IPipeline InsertAfter(string target, string name, bool processDocumentsOnce, params IModule[] modules);

        int IndexOf(string name);
        new int Count { get; }
        new IEnumerator<IPipeline> GetEnumerator();
    }
}
