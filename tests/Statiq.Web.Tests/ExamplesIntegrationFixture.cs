using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using Statiq.App;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Web.Tests
{
#if Is_Windows
    [TestFixture]
    public class ExamplesIntegrationFixture
    {
        [Test]
        public void RunsExampleProject()
        {
            // Given
            string path = Path.GetFullPath(
                Path.Combine(
                    Path.GetDirectoryName(typeof(ExamplesIntegrationFixture).Assembly.Location),
                    @"..\..\..\..\..\examples\Statiq.Web.Examples"));
            ProcessLauncher processLauncher = new ProcessLauncher("dotnet", "run")
            {
                WorkingDirectory = path
            };

            // When, Then
            processLauncher.StartNew(TestContext.Out, TestContext.Error);
        }
    }
#endif
}
