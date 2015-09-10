using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using Wyam.Common;
using NUnit.Framework;
using System.IO;

namespace Wyam.Modules.Download.Tests
{
    [TestFixture]
    public class DownloadFixture
    {
        [Test]
        public void SingleDownload()
        {
            IDocument document = Substitute.For<IDocument>();
            Stream stream = null;

            document
                .When(x => x.Clone(Arg.Any<Stream>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>(), Arg.Any<bool>()))
                .Do(x => {
                    stream = x.Arg<Stream>();
                    }
                );


            IModule download = new Download().Uris("http://www.siwawi.com/");
            IExecutionContext context = Substitute.For<IExecutionContext>();

            object result;
            context.TryConvert(new object(), out result)
                .ReturnsForAnyArgs(x =>
                {
                    x[1] = x[0];
                    return true;
                });

            // When
            download.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            stream.Seek(0, SeekOrigin.Begin);
            var content = new StreamReader(stream).ReadToEnd();
            Console.WriteLine("Content " + content);
            stream.Dispose();

        }
    }
}
