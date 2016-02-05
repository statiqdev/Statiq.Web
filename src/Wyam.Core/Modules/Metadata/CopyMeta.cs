using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

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

        /// <summary>
        /// The specified object in fromKey is copied to toKey. If a format is provided, the fromKey value is processed through string.Format before being copied (if the existing value is a DateTime, the format is passed as the argument to ToString).
        /// </summary>
        /// <param name="fromKey">The metadata key to copy from.</param>
        /// <param name="metadata">The metadata key to copy to.</param>
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

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.AsParallel().SelectMany(input =>
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
                    return new[] { input.Clone(new[] { new KeyValuePair<string, object>(_toKey, existingValue) }) };
                }
                else
                {
                    return new[] { input };
                }
            });
        }
    }
}
