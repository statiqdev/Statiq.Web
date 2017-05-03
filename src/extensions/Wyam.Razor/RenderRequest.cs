using System;
using System.IO;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Razor
{
    /// <summary>
    /// All the required parameters to render a Razor view.
    /// </summary>
    internal class RenderRequest
    {
        public Stream Input { get; set; }

        public Stream Output { get; set; }

        public string RelativePath { get; set; }

        public string ViewStartLocation { get; set; }

        public string LayoutLocation { get; set; }

        public Type BaseType { get; set; }

        public object Model { get; set; }

        public IExecutionContext Context { get; set; }

        public IDocument Document { get; set; }
    }
}