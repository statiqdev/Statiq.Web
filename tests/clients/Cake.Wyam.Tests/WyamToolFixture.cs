using Cake.Core.Diagnostics;
using Cake.Testing.Fixtures;

namespace Cake.Wyam.Tests
{
    internal sealed class WyamToolFixture : ToolFixture<WyamSettings>
    {
        public WyamToolFixture()
             : base("Wyam.exe")
        {
        }

        protected override void RunTool()
        {
            WyamRunner tool = new WyamRunner(FileSystem, Environment, ProcessRunner, Tools);
            tool.Run(Settings);
        }
    }
}