using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;

namespace Wyam.Modules.TextGeneration
{
    public class GenerateMeta : RantModule
    {
        private readonly string _key;

        public GenerateMeta(string key, object template) : base(template)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            _key = key;
        }

        public GenerateMeta(string key, ContextConfig template) : base(template)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            _key = key;
        }

        public GenerateMeta(string key, DocumentConfig template) : base(template)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            _key = key;
        }

        public GenerateMeta(string key, params IModule[] modules) : base(modules)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            _key = key;
        }

        protected override IDocument Execute(string content, IDocument input)
        {
            return input.Clone(new[] { new KeyValuePair<string, object>(_key, content) });
        }
    }
}
