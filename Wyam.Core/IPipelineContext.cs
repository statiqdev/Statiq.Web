using System.Collections.Generic;

namespace Wyam.Core
{
    public interface IPipelineContext
    {
        dynamic Metadata { get; }

        IEnumerable<dynamic> Documents { get; }

        // This gets passed from the preparation stage of a module to the execution stage of that same module
        object ExecutionObject { get; }

        Trace Trace { get; }

        IPipelineContext Clone(object persistedObject);

        bool IsReadOnly { set; }
    }
}