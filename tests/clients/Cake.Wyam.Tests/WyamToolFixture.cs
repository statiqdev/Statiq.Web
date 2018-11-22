using Cake.Common.Tools.DotNetCore;
using Cake.Core.Diagnostics;
using Cake.Testing.Fixtures;

namespace Cake.Wyam.Tests
{
    internal sealed class WyamToolFixture : DotNetCoreFixture<WyamSettings>
    {
        protected override void RunTool()
        {
            WyamRunner tool = new WyamRunner(FileSystem, Environment, ProcessRunner, Tools);
            tool.Run(Settings);
        }
    }
}