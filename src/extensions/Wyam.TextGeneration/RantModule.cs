using System;
using System.Collections.Generic;
using System.IO;
using Rant;
using Wyam.Common;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;

namespace Wyam.TextGeneration
{
    /// <summary>
    /// Base class for Rant-based text generation modules.
    /// </summary>
    public abstract class RantModule : ContentModule
    {
        private RantEngine _engine;
        private long? _seed;
        private bool _incrementSeed;

        protected RantModule(object template)
            : base(template)
        {
            SetEngine();
        }

        protected RantModule(ContextConfig template)
            : base(template)
        {
            SetEngine();
        }

        protected RantModule(DocumentConfig template)
            : base(template)
        {
            SetEngine();
        }

        protected RantModule(params IModule[] modules)
            : base(modules)
        {
            SetEngine();
        }

        private void SetEngine()
        {
            _engine = new RantEngine();
            using (Stream stream = typeof(RantModule).Assembly
                .GetManifestResourceStream(typeof(RantModule).Assembly.GetName().Name + ".Rantionary.rantpkg"))
            {
                _engine.LoadPackage(RantPackage.Load(stream));
            }
        }

        /// <summary>
        /// This allows you to set the seed used for text generation which can be handy
        /// for ensuring repeatable generations.
        /// </summary>
        /// <param name="seed">The seed to use.</param>
        public RantModule WithSeed(long seed)
        {
            _seed = seed;
            _incrementSeed = true;
            return this;
        }

        /// <summary>
        /// Specifies whether to increment the seed for each document. If incrementing
        /// is not used, every document will get the same content for the same template.
        /// </summary>
        /// <param name="increment">If set to <c>true</c> the seed will be incremented for each document.</param>
        public RantModule IncrementSeed(bool increment = true)
        {
            _incrementSeed = increment;
            return this;
        }

        /// <summary>
        /// Controls whether the dictionary will include NSFW content.
        /// </summary>
        /// <param name="includeNsfw">If set to <c>true</c> the dictionary will include NSFW content.</param>
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
            string output;
            if (_seed.HasValue)
            {
                output = _engine.Do(content.ToString(), _seed.Value);
                if (_incrementSeed)
                {
                    _seed++;
                }
            }
            else
            {
                output = _engine.Do(content.ToString());
            }
            return new[] {Execute(output, input, context)};
        }

        protected abstract IDocument Execute(string content, IDocument input, IExecutionContext context);
    }
}