using System.IO;
using System.Linq;

string[] extensions = Directory
    .GetFiles($"{Path.GetDirectoryName(ProjectFilePath)}/..", "*.nuspec", SearchOption.AllDirectories)
    .Select(x => Path.GetFileName(Path.GetDirectoryName(x)))
    .Where(x => x.Contains("Wyam.") && x != "Wyam.Windows")
    .Select(x => $"\t\tpublic static readonly KnownExtension {x.Substring(x.LastIndexOf('.') + 1)} = new KnownExtension(\"{x}\");")
    .ToArray();
string[] themes = Directory
    .GetDirectories($"{Path.GetDirectoryName(ProjectFilePath)}/../../themes", "*", SearchOption.TopDirectoryOnly)
    .Select(x => new Tuple<string, string>(Path.GetFileName(x), x))
    .SelectMany(x => Directory
        .GetDirectories(x.Item2, "*", SearchOption.TopDirectoryOnly)
        .Select(y => Path.GetFileName(y))
        .Select(y => $"\t\tpublic static readonly KnownExtension {y}Theme = new KnownExtension(\"Wyam.{x.Item1}.{y}\");"))
    .ToArray();
Output.Write($@"namespace Wyam.Configuration
{{
    public partial class KnownExtension
    {{
{string.Join(Environment.NewLine, extensions)}
{string.Join(Environment.NewLine, themes)}
    }}
}}");