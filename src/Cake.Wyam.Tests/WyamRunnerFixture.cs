using Cake.Core.Diagnostics;
using Cake.Testing.Fixtures;
using NSubstitute;

namespace Cake.Wyam.Tests
{
    internal sealed class WyamRunnerFixture : ToolFixture<WyamSettings>
    {
        public ICakeLog Log { get; set; }

        public WyamRunnerFixture()
             : base("Wyam.exe")
        {
            Log = Substitute.For<ICakeLog>();
        }

        protected override void RunTool()
        {
            WyamRunner tool = new WyamRunner(FileSystem, Environment, ProcessRunner, Globber);
            tool.Run(Settings);
        }
    }
}