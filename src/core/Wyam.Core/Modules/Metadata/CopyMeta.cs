using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Util;

namespace Wyam.Core.Modules.Metadata
{
    /// <summary>
    /// Copies the specified meta key to a new meta key, with an optional format argument.
    /// </summary>
    /// <category>Metadata</category>
    public class CopyMeta : IModule
    {
        private readonly string _fromKey;
        private readonly string _toKey;
        private string _format;
        private Func<string, string> _execute;

        /// <summary>
        /// The specified object in fromKey is copied to toKey. If a format is provided, the fromKey value is processed through string.Format before being copied (if the existing value is a DateTime, the format is passed as the argument to ToString).
        /// </summary>
        /// <param name="fromKey">The metadata key to copy from.</param>
        /// <param name="toKey">The metadata key to copy to.</param>
        /// <param name="format">The formatting to apply to the new value.</param>
        public CopyMeta(string fromKey, string toKey, string format = null)
        {
            if (fromKey == null)
            {
                throw new ArgumentNullException(nameof(fromKey));
            }
            if (toKey == null)
            {
                throw new ArgumentNullException(nameof(toKey));
            }

            _fromKey = fromKey;
            _toKey = toKey;
            _format = format;
        }

        public CopyMeta WithFormat(string format)
        {
            if (format == null)
            {
                throw new ArgumentNullException(nameof(format));
            }
            _format = format;
            return this;
        }

        public CopyMeta WithFormat(Func<string,string> execute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }
            _execute = execute;
            return this;
        }

        /// <inheritdoc />
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.AsParallel().SelectMany(context, input =>
            {
                object existingValue;
                var hasExistingKey = input.TryGetValue(_fromKey, out existingValue);

                if (hasExistingKey)
                {
                    if (_format != null)
                    {
                        if (existingValue is DateTime)
                        {
                            existingValue = ((DateTime)existingValue).ToString(_format);
                        }
                        else
                        {
                            existingValue = string.Format(_format, existingValue);
                        }
                    }

                    if (_execute != null)
                    {
                        existingValue = _execute.Invoke(existingValue.ToString());
                    }

                    return new[] { context.GetDocument(input, new[] { new KeyValuePair<string, object>(_toKey, existingValue) }) };
                }
                else
                {
                    return new[] { input };
                }
            });
        }
    }
}
