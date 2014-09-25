using System;
using System.Collections.Generic;
using Wyam.Core;

namespace Wyam.Modules.Basic
{
    // This module is helpful when one-off behavior is needed without writing a whole extension module
    public class Delegates : IModule
    {
        private readonly Func<PipelineContext, IEnumerable<PipelineContext>> _prepareFunc;
        private readonly Func<PipelineContext, string, string> _executeFunc;

        public Delegates(Func<PipelineContext, IEnumerable<PipelineContext>> prepareFunc, Func<PipelineContext, string, string> executeFunc)
        {
            _prepareFunc = prepareFunc;
            _executeFunc = executeFunc;
        }

        public IEnumerable<PipelineContext> Prepare(PipelineContext context)
        {
            if (_prepareFunc != null)
            {
                return _prepareFunc(context);
            }
            return null;
        }

        public string Execute(PipelineContext context, string content)
        {
            if (_executeFunc != null)
            {
                return _executeFunc(context, content);
            }
            return content;
        }
    }
}