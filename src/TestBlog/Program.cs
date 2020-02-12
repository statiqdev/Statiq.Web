using System;
using System.Linq;
using System.Threading.Tasks;
using Statiq.App;
using Statiq.Common;
using Statiq.Web;

namespace TestBlog
{
    public static class Program
    {
        public static async Task<int> Main(string[] args) =>
            await Bootstrapper.Factory
                .CreateWeb(args)
                .AddSetting(
                    Keys.DestinationPath,
                    Config.FromDocument(
                        doc => doc.Source.Directory.Segments.Last().SequenceEqual("posts".AsMemory())
                            ? new DirectoryPath("blog")
                                .Combine(new DirectoryPath(doc.Get<DateTime>("Published").ToString("yyyy/MM/dd")))
                                .CombineFile(doc.Destination.FileName.ChangeExtension(".html"))
                            : doc.Destination.ChangeExtension(".html")))
                .RunAsync();
    }
}
