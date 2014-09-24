using System;
using System.Collections.Generic;

namespace Wyam.Core.Tests
{
    public class DelegateModule : IModule
    {
        public Func<PipelineContext, IEnumerable<PipelineContext>> PrepareFunc { get; set; }

        public Func<PipelineContext, string, string> ExecuteFunc { get; set; }

        public IEnumerable<PipelineContext> Prepare(PipelineContext context)
        {
            if (PrepareFunc != null)
            {
                return PrepareFunc(context);
            }
            return null;
        }

        public string Execute(PipelineContext context, string content)
        {
            if (ExecuteFunc != null)
            {
                return ExecuteFunc(context, content);
            }
            return content;
        }
    }
}