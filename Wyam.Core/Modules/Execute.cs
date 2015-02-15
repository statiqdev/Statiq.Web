using System;
using System.Collections.Generic;
using Wyam.Core;

namespace Wyam.Core.Modules
{
    // This module is helpful when one-off behavior is needed without writing a whole extension module
    internal class Execute : IModule
    {
        private readonly Func<PipelineContext, IEnumerable<PipelineContext>> _prepare;
        private readonly Func<PipelineContext, string, string> _execute;

        public Execute(Func<PipelineContext, IEnumerable<PipelineContext>> prepare, Func<PipelineContext, string, string> execute)
        {
            _prepare = prepare;
            _execute = execute;
        }

        public IEnumerable<PipelineContext> Prepare(PipelineContext context)
        {
            if (_prepare != null)
            {
                return _prepare(context);
            }
            return null;
        }

        string IModule.Execute(PipelineContext context, string content)
        {
            if (_execute != null)
            {
                return _execute(context, content);
            }
            return content;
        }
    }
}