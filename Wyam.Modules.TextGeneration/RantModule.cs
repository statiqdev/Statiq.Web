using System;
using System.Collections.Generic;
using System.IO;
using Rant;
using Wyam.Common;

namespace Wyam.Modules.TextGeneration
{
    public abstract class RantModule : ContentModule
    {
        private RantEngine _engine;
        private long? _seed;

        protected RantModule(object template) : base(template)
        {
            SetEngine();
        }

        protected RantModule(ContextConfig template) : base(template)
        {
            SetEngine();
        }

        protected RantModule(DocumentConfig template) : base(template)
        {
            SetEngine();
        }

        protected RantModule(params IModule[] modules) : base(modules)
        {
            SetEngine();
        }

        private void SetEngine()
        {
            _engine = new RantEngine();
            using (Stream stream = typeof(RantModule).Assembly.GetManifestResourceStream("Rantionary.rantpkg"))
            {
                _engine.LoadPackage(RantPackage.Load(stream));
            }
        }

        // Allows you to set a seed for repeatability and testing
        public RantModule SetSeed(long seed)
        {
            _seed = seed;
            return this;
        }

        public RantModule IncludeNsfw(bool includeNsfw = true)
        {
            if (includeNsfw)
            {
                _engine.Dictionary.IncludeHiddenClass("nsfw");
            }
            else
            {
                _engine.Dictionary.ExcludeHiddenClass("nsfw");
            }
            return this;
        }

        protected override IEnumerable<IDocument> Execute(object content, IDocument input, IExecutionContext context)
        {
            string output = _seed.HasValue
                ? _engine.Do(content.ToString(), _seed.Value)
                : _engine.Do(content.ToString());
            return new[] {Execute(output, input)};
        }

        protected abstract IDocument Execute(string content, IDocument input);
    }
}