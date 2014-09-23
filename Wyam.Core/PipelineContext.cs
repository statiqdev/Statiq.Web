using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    internal class PipelineContext
    {
        private readonly Engine _engine;
        private readonly VariableStack _variables;

        public PipelineContext(Engine engine, VariableStack variables)
        {
            _engine = engine;
            _variables = variables;
        }
    }
}
