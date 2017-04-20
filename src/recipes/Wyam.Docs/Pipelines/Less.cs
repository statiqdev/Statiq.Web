using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.IO;

namespace Wyam.Docs.Pipelines
{
    /// <summary>
    /// Processes any Less stylesheets and outputs the resulting CSS files.
    /// </summary>
    public class Less : Pipeline
    {
        internal Less()
            : base(GetModules())
        {
        }

        private static ModuleList GetModules() => new ModuleList
        {
            new ReadFiles("assets/css/*.less"),
            new Concat(
                new ReadFiles("assets/css/bootstrap/bootstrap.less")
            ),
            new Concat(
                new ReadFiles("assets/css/adminlte/AdminLTE.less")
            ),
            new Concat(
                new ReadFiles("assets/css/theme/theme.less")
            ),
            new Wyam.Less.Less(),
            new WriteFiles(".css")
        };
    }
}
