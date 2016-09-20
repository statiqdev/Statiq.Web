using System.IO;
using System.Linq;

string[] extensions = Directory
    .GetFiles($"{Path.GetDirectoryName(ProjectFilePath)}/..", "*.nuspec", SearchOption.AllDirectories)
    .Select(x => Path.GetFileName(Path.GetDirectoryName(x)))
    .Where(x => x.Contains("Wyam.") && x != "Wyam.Windows")
    .Select(x => $"\t\tpublic static readonly KnownExtension {x.Substring(x.LastIndexOf('.') + 1)} = new KnownExtension(\"{x}\");")
    .ToArray();
Output.Write($@"namespace Wyam.Configuration
{{
    public partial class KnownExtension
    {{
{string.Join(Environment.NewLine, extensions)}
    }}
}}");