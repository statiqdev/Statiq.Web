using System;
using System.Collections.Generic;
using Wyam.Core;

namespace Wyam.Core.Modules
{
    // This module is helpful when one-off behavior is needed without writing a whole extension module
    internal class Execute : IModule
    {
        private readonly Func<IPipelineContext, IEnumerable<IPipelineContext>> _prepare;
        private readonly Func<IPipelineContext, string, string> _execute;

        public Execute(Func<IPipelineContext, IEnumerable<IPipelineContext>> prepare, Func<IPipelineContext, string, string> execute)
        {
            _prepare = prepare;
            _execute = execute;
        }

        public IEnumerable<IPipelineContext> Prepare(IPipelineContext context)
        {
            if (_prepare != null)
            {
                return _prepare(context);
            }
            return null;
        }

        string IModule.Execute(IPipelineContext context, string content)
        {
            if (_execute != null)
            {
                return _execute(context, content);
            }
            return content;
        }
    }
}