using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Testing;
using Wyam.Testing.Execution;
using Wyam.Testing.Meta;

namespace Wyam.Common.Tests.Execution
{
    [TestFixture]
    public class LinkExtensionsFixture : BaseFixture
    {
        public class GetLinkTests : LinkExtensionsFixture
        {
            [TestCase("http://foo.com/bar", false, "http://foo.com/bar")]
            [TestCase("http://foo.com/bar", true, "http://foo.com/bar")]
            [TestCase("https://foo.com/bar", false, "https://foo.com/bar")]
            [TestCase("https://foo.com/bar", true, "https://foo.com/bar")]
            [TestCase("foo/bar", false, "/foo/bar")]
            [TestCase("foo/bar", true, "http://domain.com/foo/bar")]
            [TestCase("/foo/bar", false, "/foo/bar")]
            [TestCase("/foo/bar", true, "http://domain.com/foo/bar")]
            [TestCase("//foo/bar", false, "//foo/bar")]
            [TestCase("//foo/bar", true, "http://domain.com//foo/bar")]
            public void UsesAbsoluteLinkIfProvided(string value, bool includeHost, string expected)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.Host] = "domain.com";
                TestMetadata metadata = new TestMetadata
                {
                    { "Path", value }
                };
                metadata.AddTypeConversion<string, FilePath>(x => new FilePath(x));

                // When
                string result = context.GetLink(metadata, "Path", includeHost);

                // Then
                result.ShouldBe(expected);
            }
        }
    }
}
