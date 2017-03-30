using System;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.IO;

namespace Wyam.Configuration.Tests
{
    public class MetadataTestRecipe : IRecipe
    {
        public void Apply(IEngine engine)
        {
            engine.Settings["Foo"] = "Bar";
        }

        public void Scaffold(IFile configFile, IDirectory inputDirectory)
        {
            throw new NotImplementedException();
        }
    }
}