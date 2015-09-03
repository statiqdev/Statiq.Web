using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common
{
    // This class satisfies a common use case for modules where you need to get some configuration value
    // either directly, from a delegate at the module level, or from a delegate at a per-document level
    // and the user should be able to specify any of these possibilities
    public class ConfigHelper<T>
    {
        private readonly ContextConfig _contextConfig;
        private readonly ContextConfig<T> _typedContextConfig;
        private readonly DocumentConfig _documentConfig;
        private readonly DocumentConfig<T> _typedDocumentConfig;
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

        public ConfigHelper(ContextConfig<T> config, T defaultValue = default(T))
        {
            _typedContextConfig = config;
            _defaultValue = defaultValue;
        }

        public ConfigHelper(DocumentConfig config, T defaultValue = default(T))
        {
            _documentConfig = config;
            _defaultValue = defaultValue;
        }

        public ConfigHelper(DocumentConfig<T> config, T defaultValue = default(T))
        {
            _typedDocumentConfig = config;
            _defaultValue = defaultValue;
        }

        // Call this each time you need the value, passing in a postProcessing function if required
        // If no document delegate is specified, then this will get and cache the value on first request
        public T GetValue(IDocument document, IExecutionContext context, Func<T, T> postProcessing = null)
        {
            if (_documentConfig == null && _typedDocumentConfig == null)
            {
                if (_gotValue)
                {
                    return _value;
                }
                _value = (_contextConfig == null && _typedContextConfig == null) 
                    ? _defaultValue : (_typedContextConfig == null ? _contextConfig.Invoke<T>(context) : _typedContextConfig(context));
                if (postProcessing != null)
                {
                    _value = postProcessing(_value);
                }
                _gotValue = true;
                return _value;
            }

            T value = _typedDocumentConfig == null ? _documentConfig.Invoke<T>(document, context) : _typedDocumentConfig(document, context);
            if (postProcessing != null)
            {
                value = postProcessing(value);
            }
            return value;
        }
    }
}
