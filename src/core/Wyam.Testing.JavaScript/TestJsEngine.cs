using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Jint;

namespace Wyam.Testing.JavaScript
{
    public static class TestJsEngine
    {
        public static IJsEngine Create()
        {
            return new JintJsEngine();
        }
    }
}
