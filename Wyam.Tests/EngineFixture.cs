using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core;


namespace Wyam.Tests
{
    [TestFixture]
    public class EngineFixture
    {
        [Test]
        public void ConfigureSetPrimitiveVariables()
        {
            Engine engine = new Engine();
            engine.Configure(@"
                vars.TestString = ""teststring"";
                vars.TestInt = 1234;
                vars.TestFloat = 1234.567;
                vars.TestBool = true;
            ");
        }
    }
}
