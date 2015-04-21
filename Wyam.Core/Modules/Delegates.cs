using System;
using System.Collections.Generic;
using Wyam.Core;

namespace Wyam.Core.Modules
{
    // This module is helpful when one-off behavior is needed without writing a whole extension module
    public class Delegates : Module
    {
        private readonly Func<IPipelineContext, IEnumerable<IPipelineContext>> _prepare;
        private readonly Func<IPipelineContext, string, string> _execute;

        public Delegates(Func<IPipelineContext, IEnumerable<IPipelineContext>> prepare, Func<IPipelineContext, string, string> execute)
        {
            _prepare = prepare;
            _execute = execute;
        }

        protected internal override IEnumerable<IPipelineContext> Prepare(IPipelineContext context)
        {
            return _prepare != null ? _prepare(context) : null;
        }

        protected internal override string Execute(IPipelineContext context, string content)
        {
            return _execute != null ? _execute(context, content) : content;
        }
    }
}