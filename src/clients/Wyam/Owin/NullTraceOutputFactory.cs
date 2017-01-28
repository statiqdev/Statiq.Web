using System.IO;

using Microsoft.Owin.Hosting.Tracing;

namespace Wyam.Owin
{
    public class NullTraceOutputFactory : ITraceOutputFactory
    {
        public TextWriter Create(string outputFile)
        {
            return StreamWriter.Null;
        }
    }
}