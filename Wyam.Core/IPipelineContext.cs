using System.Collections.Generic;

namespace Wyam.Core
{
    // A pipeline context is immutable, call .Clone() to get a new context with persisted object and/or new metadata items
    public interface IPipelineContext
    {
        IMetadata Metadata { get; }

        // This contains the metadata for all previous pipelines
        // It is populated at the conclusion of each pipeline prepare in sequence
        IEnumerable<IMetadata> AllMetadata { get; }

        // This gets passed from the preparation stage of a module to the execution stage of that same module
        object PersistedObject { get; }

        Trace Trace { get; }

        IPipelineContext Clone(object persistedObject, IEnumerable<KeyValuePair<string, object>> items = null);
        IPipelineContext Clone(IEnumerable<KeyValuePair<string, object>> items = null);
    }
}