using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Configuration.Preprocessing;
using Wyam.Testing;

namespace Wyam.Configuration.Tests.Preprocessing
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class ArgumentSplitterTests : BaseFixture
    {
        public class SplitMethodTests : ArgumentSplitterTests
        {
            // From http://stackoverflow.com/a/298990/807064
            [TestCase("", new string[] {})]
            [TestCase("a", new [] { "a" })]
            [TestCase(" abc ", new[] { "abc" })]
            [TestCase("a b ", new[] { "a", "b" })]
            [TestCase("a b \"c d\"", new[] { "a", "b", "c d" })]
            [TestCase(@"/src:""C:\tmp\Some Folder\Sub Folder"" /users:""abcdefg@hijkl.com"" tasks:""SomeTask,Some Other Task"" -someParam",
                new [] { @"/src:""C:\tmp\Some Folder\Sub Folder""", @"/users:""abcdefg@hijkl.com""", @"tasks:""SomeTask,Some Other Task""", @"-someParam" })]
            public void ShouldSplitExceptInQuotes(string arguments, string[] expected)
            {
                // Given, When
                IEnumerable<string> actual = ArgumentSplitter.Split(arguments);

                // Then
                CollectionAssert.AreEqual(expected, actual);
            }
        }
    }
}
