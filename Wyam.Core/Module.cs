using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    // Modules should be stateless - they are often run more than once
    public abstract class Module
    {
        // This should primarily set the metadata and read (but not process) any required resources, parsing for additional metadata if appropriate
        internal protected virtual IEnumerable<IModuleContext> Prepare(IModuleContext context)
        {
            yield return context;
        }

        // This should transform the provided content and return the transformed content using the metadata and persistent object provided to the context during prepare
        internal protected virtual string Execute(IModuleContext context, string content)
        {
            return content;
        }
    }

    // Modules should be stateless - they are often run more than once
    public abstract class AggregateModule
    {
        // This should primarily set the metadata and read (but not process) any required resources, parsing for additional metadata if appropriate
        internal protected virtual IEnumerable<IModuleContext> Prepare(IEnumerable<IModuleContext> contexts)
        {
            return contexts;
        }

        // This should transform the provided content and return the transformed content using the metadata and persistent object provided to the context during prepare
        internal protected virtual string Execute(IModuleContext context, string content)
        {
            return content;
        }
    }
}
