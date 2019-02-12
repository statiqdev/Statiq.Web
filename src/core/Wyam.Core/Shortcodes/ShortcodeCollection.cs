using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Shortcodes;

namespace Wyam.Core.Shortcodes
{
    internal class ShortcodeCollection : IShortcodeCollection
    {
        private readonly Dictionary<string, Func<IShortcode>> _shortcodes =
            new Dictionary<string, Func<IShortcode>>(StringComparer.OrdinalIgnoreCase);

        public IShortcode CreateInstance(string name) => _shortcodes[name]();

        public void Add<TShortcode>(string name)
            where TShortcode : IShortcode
        {
            if (string.IsNullOrWhiteSpace(name) || name.Any(c => char.IsWhiteSpace(c)))
            {
                throw new ArgumentException(nameof(name));
            }
            _shortcodes[name] = () => Activator.CreateInstance<TShortcode>();
        }

        public void Add<TShortcode>()
            where TShortcode : IShortcode =>
            Add<TShortcode>(typeof(TShortcode).Name);

        public void Add(string name, Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            if (!typeof(IShortcode).IsAssignableFrom(type))
            {
                throw new ArgumentException("The type must implement " + nameof(IShortcode), nameof(type));
            }
            if (string.IsNullOrWhiteSpace(name) || name.Any(c => char.IsWhiteSpace(c)))
            {
                throw new ArgumentException(nameof(name));
            }
            _shortcodes[name] = () => (IShortcode)Activator.CreateInstance(type);
        }

        public void Add(Type type) => Add(type?.Name, type);

        public void Add(string name, string result) =>
            Add(name, (args, content, doc, ctx) => result != null ? ctx.GetShortcodeResult(ctx.GetContentStream(result)) : null);

        public void Add(string name, ContextConfig contextConfig) =>
            Add(name, (args, content, doc, ctx) =>
            {
                string result = contextConfig?.Invoke<string>(ctx);
                return result != null ? ctx.GetShortcodeResult(ctx.GetContentStream(result)) : null;
            });

        public void Add(string name, DocumentConfig documentConfig) =>
            Add(name, (args, content, doc, ctx) =>
            {
                string result = documentConfig?.Invoke<string>(doc, ctx);
                return result != null ? ctx.GetShortcodeResult(ctx.GetContentStream(result)) : null;
            });

        public void Add(string name, Func<string, string> func) =>
            Add(name, (args, content, doc, ctx) =>
            {
                string result = func?.Invoke(content);
                return result != null ? ctx.GetShortcodeResult(ctx.GetContentStream(result)) : null;
            });

        public void Add(string name, Func<KeyValuePair<string, string>[], string> func) =>
            Add(name, (args, content, doc, ctx) =>
            {
                string result = func?.Invoke(args);
                return result != null ? ctx.GetShortcodeResult(ctx.GetContentStream(result)) : null;
            });

        public void Add(string name, Func<KeyValuePair<string, string>[], string, string> func) =>
            Add(name, (args, content, doc, ctx) =>
            {
                string result = func?.Invoke(args, content);
                return result != null ? ctx.GetShortcodeResult(ctx.GetContentStream(result)) : null;
            });

        public void Add(string name, Func<KeyValuePair<string, string>[], string, IExecutionContext, string> func) =>
            Add(name, (args, content, doc, ctx) =>
            {
                string result = func?.Invoke(args, content, ctx);
                return result != null ? ctx.GetShortcodeResult(ctx.GetContentStream(result)) : null;
            });

        public void Add(string name, Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, string> func) =>
            Add(name, (args, content, doc, ctx) =>
            {
                string result = func?.Invoke(args, content, doc, ctx);
                return result != null ? ctx.GetShortcodeResult(ctx.GetContentStream(result)) : null;
            });

        public void Add(string name, Func<KeyValuePair<string, string>[], IExecutionContext, string> func) =>
            Add(name, (args, content, doc, ctx) =>
            {
                string result = func?.Invoke(args, ctx);
                return result != null ? ctx.GetShortcodeResult(ctx.GetContentStream(result)) : null;
            });

        public void Add(string name, Func<KeyValuePair<string, string>[], IDocument, IExecutionContext, string> func) =>
            Add(name, (args, content, doc, ctx) =>
            {
                string result = func?.Invoke(args, doc, ctx);
                return result != null ? ctx.GetShortcodeResult(ctx.GetContentStream(result)) : null;
            });

        public void Add(string name, Func<string, IExecutionContext, string> func) =>
            Add(name, (args, content, doc, ctx) =>
            {
                string result = func?.Invoke(content, ctx);
                return result != null ? ctx.GetShortcodeResult(ctx.GetContentStream(result)) : null;
            });

        public void Add(string name, Func<string, IDocument, IExecutionContext, string> func) =>
            Add(name, (args, content, doc, ctx) =>
            {
                string result = func?.Invoke(content, doc, ctx);
                return result != null ? ctx.GetShortcodeResult(ctx.GetContentStream(result)) : null;
            });

        public void Add(string name, Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, IShortcodeResult> func)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Any(c => char.IsWhiteSpace(c)))
            {
                throw new ArgumentException(nameof(name));
            }
            _shortcodes[name] = () => new FuncShortcode(func);
        }

        public int Count => _shortcodes.Count;

        public bool Contains(string name) => _shortcodes.ContainsKey(name);

        public IEnumerator<string> GetEnumerator() => _shortcodes.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
