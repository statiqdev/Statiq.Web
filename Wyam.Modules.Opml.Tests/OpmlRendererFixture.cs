using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit;
using NUnit.Framework;
using Wyam.Common;

namespace Wyam.Modules.Opml.Tests
{
    [TestFixture]
    public class OpmlRendererFixture
    {
        [Test]
        public void SimpleReplacementOutput()
        {
            var opml = new OpmlRenderer().Download("http://hosting.opml.org/dave/spec/placesLived.opml");

            var result = opml.Execute(new IDocument[] { }, null).ToList();
        }
    }
}
