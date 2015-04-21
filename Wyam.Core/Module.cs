using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    // Modules should be stateless - they can potentially be run more than once
    public abstract class Module
    {
        // This should primarily set the metadata and read (but not process) any required resources, parsing for additional metadata if appropriate
        internal protected virtual IEnumerable<IPipelineContext> Prepare(IPipelineContext context)
        {
            yield return context;
        }

        // This should transform the provided content and return the transformed content using the metadata and persistent object provided to the context during prepare
        internal protected virtual string Execute(IPipelineContext context, string content)
        {
            return content;
        }
    }
}
