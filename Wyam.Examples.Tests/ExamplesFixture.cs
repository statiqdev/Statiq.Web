using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Core;

namespace Wyam.Examples.Tests
{
    [TestFixture(Category = "ExcludeFromAppVeyor")]
    public class ExamplesFixture
    {
        [Test]
        public void ExecuteAllExamples()
        {
            string path = Path.Combine(Assembly.GetExecutingAssembly().Location, "Examples");
            while (!Directory.Exists(path))
            {
                path = Directory.GetParent(path).Parent.FullName;
                path = Path.Combine(path, "Examples");
            }
            int count = 0;
            foreach(string example in Directory.EnumerateDirectories(path))
            {
                Console.WriteLine("Executing example " + example);
                Engine engine = new Engine
                {
                    RootFolder = example
                };
                string config = Path.Combine(example, "config.wyam");
                if (File.Exists(config))
                {
                    Console.WriteLine("Loading configuration file");
                    engine.Configure(File.ReadAllText(config));
                }
                engine.Execute();
                count++;
            }
            Assert.AreEqual(4, count);
        }
    }
}
