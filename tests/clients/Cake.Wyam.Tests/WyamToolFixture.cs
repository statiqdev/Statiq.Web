using Cake.Core.Diagnostics;
using Cake.Testing.Fixtures;
using NSubstitute;

namespace Cake.Wyam.Tests
{
    internal sealed class WyamToolFixture : ToolFixture<WyamSettings>
    {
        public ICakeLog Log { get; set; }

        public WyamToolFixture()
             : base("Wyam.exe")
        {
            Log = Substitute.For<ICakeLog>();
        }

        protected override void RunTool()
        {
            WyamRunner tool = new WyamRunner(FileSystem, Environment, ProcessRunner, Tools);
            tool.Run(Settings);
        }
    }
}