using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common
{
    // This class satisfies a common use case for modules where you need to get some configuration value
    // either directly, from a function at the module level, or from a function at a per-document level
    // and the user should be able to specify any of these possibilities
    public class ModuleConstructorHelper<T>
    {
        private readonly Func<IExecutionContext, T> _contextFunc;
        private readonly Func<IDocument, IExecutionContext, T> _documentFunc;
        private readonly T _defaultValue;
        private T _value;
        private bool _gotValue;

        public ModuleConstructorHelper(T value)
        {
            _contextFunc = c => value;
        }

        // defaultValue is used if the func is null
        public ModuleConstructorHelper(Func<IExecutionContext, T> func, T defaultValue = default(T))
        {
            _contextFunc = func;
            _defaultValue = defaultValue;
        }

        // defaultValue is used if the func is null
        public ModuleConstructorHelper(Func<IDocument, IExecutionContext, T> func, T defaultValue = default(T))
        {
            _documentFunc = func;
            _defaultValue = defaultValue;
        }

        // Call this each time you need the value, passing in a postProcessing function if required
        // If no document func is specified, then this will get and cache the value on first request
        public T GetValue(IDocument document, IExecutionContext context, Func<T, T> postProcessing = null)
        {
            if (_documentFunc == null)
            {
                if (_gotValue)
                {
                    return _value;
                }
                _value = _contextFunc == null ? _defaultValue : _contextFunc(context);
                if (postProcessing != null)
                {
                    _value = postProcessing(_value);
                }
                _gotValue = true;
                return _value;
            }

            T value = _documentFunc(document, context);
            if (postProcessing != null)
            {
                value = postProcessing(value);
            }
            return value;
        }
    }
}
