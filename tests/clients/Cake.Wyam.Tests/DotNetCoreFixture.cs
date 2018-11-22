using Cake.Common.Tools.DotNetCore;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Testing.Fixtures;

namespace Cake.Wyam.Tests
{
    // From Cake - can be removed if this class is ever made public
    internal abstract class DotNetCoreFixture<TSettings> : ToolFixture<TSettings, ToolFixtureResult>
        where TSettings : DotNetCoreSettings, new()
    {
        protected DotNetCoreFixture()
            : base("Wyam.dll")
        {
            ProcessRunner.Process.SetStandardOutput(new string[] { });
        }

        protected override ToolFixtureResult CreateResult(FilePath path, ProcessSettings process)
        {
            return new ToolFixtureResult(path, process);
        }
    }
}