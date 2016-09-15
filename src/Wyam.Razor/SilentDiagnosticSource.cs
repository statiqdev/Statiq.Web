using Wyam.Common.Tracing;

namespace Wyam.Razor
{
    internal class SilentDiagnosticSource : System.Diagnostics.DiagnosticSource
    {
        public override void Write(string name, object value)
        {
            // Do nothing
        }

        public override bool IsEnabled(string name) => true;
    }
}