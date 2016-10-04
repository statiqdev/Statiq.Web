using System.IO;
using System.Linq;
using System.Collections.Generic;

string[] recipes = Directory
    .GetDirectories($"{Path.GetDirectoryName(ProjectFilePath)}/../../../themes", "*", SearchOption.TopDirectoryOnly)
    .Select(x => new Tuple<string, string>(Path.GetFileName(x), x))
    .SelectMany(x => Directory
        .GetDirectories(x.Item2, "*", SearchOption.TopDirectoryOnly)
        .Select(y => Path.GetFileName(y))
        .Select(y => $"\t\tpublic static readonly KnownTheme {y} = new KnownTheme(nameof(KnownRecipe.{x.Item1}), null, new[] {{ \"Wyam.{x.Item1}.{y}\" }});"))
    .ToArray();
Output.Write($@"namespace Wyam.Configuration
{{
    public partial class KnownTheme
    {{
{string.Join(Environment.NewLine, recipes)}
    }}
}}");