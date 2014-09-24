using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    // Modules should be stateless - they can potentially be run more than once
    public interface IModule
    {
        // This should primarily set the metadata and read (but not process) any required resources, parsing for additional metadata if appropriate
        // The context will need to be cloned before the metadata can be changed (the input context is locked)
        // This is to remind implementors to clone the metadata for each returned context instead of changing the same one over and over
        IEnumerable<PipelineContext> Prepare(PipelineContext context);

        // This should transform the provided content and return the transformed content using the metadata and persistent object provided in the context
        string Execute(PipelineContext context, string content);
    }
}
