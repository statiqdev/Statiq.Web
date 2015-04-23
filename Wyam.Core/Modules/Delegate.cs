using System;
using System.Collections.Generic;
using Wyam.Core;

namespace Wyam.Core.Modules
{
    // This module is helpful when one-off behavior is needed without writing a whole extension module
    public class Delegate : Module
    {
        private readonly Func<IModuleContext, IEnumerable<IModuleContext>> _prepare;
        private readonly Func<IModuleContext, string, string> _execute;

        public Delegate(Func<IModuleContext, IEnumerable<IModuleContext>> prepare, Func<IModuleContext, string, string> execute)
        {
            _prepare = prepare;
            _execute = execute;
        }

        protected internal override IEnumerable<IModuleContext> Prepare(IModuleContext context)
        {
            return _prepare != null ? _prepare(context) : null;
        }

        protected internal override string Execute(IModuleContext context, string content)
        {
            return _execute != null ? _execute(context, content) : content;
        }
    }
}