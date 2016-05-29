using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using NuGet.Packaging;
using NuGet.ProjectManagement;
using Wyam.Common.Tracing;

namespace Wyam.Configuration.NuGet
{
    internal class NuGetProjectContext : INuGetProjectContext
    {
        public void Log(MessageLevel level, string message, params object[] args)
        {
            switch (level)
            {
                case MessageLevel.Warning:
                    Trace.Warning(message, args);
                    break;
                case MessageLevel.Error:
                    Trace.Error(message, args);
                    break;
                default:
                    Trace.Verbose(message, args);
                    break;
            }
        }

        public FileConflictAction ResolveFileConflict(string message)
        {
            return FileConflictAction.Ignore;
        }

        public PackageExtractionContext PackageExtractionContext { get; set; }

        public XDocument OriginalPackagesConfig { get; set; }

        public ISourceControlManagerProvider SourceControlManagerProvider => null;

        public ExecutionContext ExecutionContext => null;

        public void ReportError(string message)
        {
        }

        public NuGetActionType ActionType { get; set; }
    }
}
