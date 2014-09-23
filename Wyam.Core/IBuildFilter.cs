using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wyam.Core
{
    // Controls which inputs and/or pipelines are used in this build
    // Useful for controlling builds on changes such as a file watcher to rebuild only those files that have changed
    public interface IBuildFilter
    {
    }
}
