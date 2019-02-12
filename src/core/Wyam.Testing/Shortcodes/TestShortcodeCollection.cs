using System;
using System.Collections.Generic;
using System.Text;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Shortcodes;

namespace Wyam.Testing.Shortcodes
{
    public class TestShortcodeCollection : Dictionary<string, Type>, IShortcodeCollection
    {
        public TestShortcodeCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public IShortcode CreateInstance(string name) => (IShortcode)Activator.CreateInstance(this[name]);

        public void Add<TShortcode>(string name)
            where TShortcode : IShortcode =>
            this[name] = typeof(TShortcode);

        public void Add(string name, string result) =>
            throw new NotImplementedException();

        public void Add(string name, ContextConfig contextConfig) =>
            throw new NotImplementedException();

        public void Add(string name, DocumentConfig documentConfig) =>
            throw new NotImplementedException();

        public void Add(string name, Func<string, string> func) =>
            throw new NotImplementedException();

        public void Add(string name, Func<KeyValuePair<string, string>[], string> func) =>
            throw new NotImplementedException();

        public void Add(string name, Func<KeyValuePair<string, string>[], string, string> func) =>
            throw new NotImplementedException();

        public void Add(string name, Func<KeyValuePair<string, string>[], string, IExecutionContext, string> func) =>
            throw new NotImplementedException();

        public void Add(string name, Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, string> func) =>
            throw new NotImplementedException();

        public void Add(string name, Func<KeyValuePair<string, string>[], IExecutionContext, string> func) =>
            throw new NotImplementedException();

        public void Add(string name, Func<KeyValuePair<string, string>[], IDocument, IExecutionContext, string> func) =>
            throw new NotImplementedException();

        public void Add(string name, Func<string, IExecutionContext, string> func) =>
            throw new NotImplementedException();

        public void Add(string name, Func<string, IDocument, IExecutionContext, string> func) =>
            throw new NotImplementedException();

        public void Add(string name, Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, IShortcodeResult> func) =>
            throw new NotImplementedException();

        public bool Contains(string name) => ContainsKey(name);

        IEnumerator<string> IEnumerable<string>.GetEnumerator() => Keys.GetEnumerator();
    }
}
