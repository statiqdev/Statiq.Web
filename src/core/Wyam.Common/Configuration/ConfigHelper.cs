using System;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Configuration
{
    // This class satisfies a common use case for modules where you need to get some configuration value
    // either directly, from a delegate at the module level, or from a delegate at a per-document level
    // and the user should be able to specify any of these possibilities
    public class ConfigHelper<T>
    {
        private readonly ContextConfig _contextConfig;
        private readonly DocumentConfig _documentConfig;
        private readonly T _defaultValue;
        private T _value;
        private bool _gotValue;

        public ConfigHelper(T value)
        {
            _contextConfig = c => value;
        }

        // defaultValue is used if the delegate is null
        public ConfigHelper(ContextConfig config, T defaultValue = default(T))
        {
            _contextConfig = config;
            _defaultValue = defaultValue;
        }

        public ConfigHelper(DocumentConfig config, T defaultValue = default(T))
        {
            _documentConfig = config;
            _defaultValue = defaultValue;
        }

        // Call this each time you need the value, passing in a postProcessing function if required
        // If no document delegate is specified, then this will get and cache the value on first request
        public T GetValue(IDocument document, IExecutionContext context, Func<T, T> postProcessing = null)
        {
            if (_documentConfig == null)
            {
                if (_gotValue)
                {
                    return _value;
                }
                _value = _contextConfig == null ? _defaultValue : _contextConfig.Invoke<T>(context);
                if (postProcessing != null)
                {
                    _value = postProcessing(_value);
                }
                _gotValue = true;
                return _value;
            }

            T value = _documentConfig.Invoke<T>(document, context);
            if (postProcessing != null)
            {
                value = postProcessing(value);
            }
            return value;
        }
    }
}
