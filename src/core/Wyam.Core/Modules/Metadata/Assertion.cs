using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Util;

namespace Wyam.Core.Modules.Metadata
{
    internal class Assertion<T>
    {
        private readonly Func<T, bool> _execute;

        public string Message { get; }

        public Assertion(Func<T, bool> execute, string message)
        {
            _execute = execute;
            Message = message;
        }

        public bool Execute(T value)
        {
            return _execute(value);
        }
    }
}
