using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Wyam.Core.Syndication.Atom
{
    /// <summary>
    /// http://tools.ietf.org/html/rfc4287#section-3.3
    /// </summary>
    [Serializable]
    public struct AtomDate
    {
        private DateTime? _value;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="date"></param>
        public AtomDate(DateTime date)
        {
            _value = date;
        }

        [XmlIgnore]
        public DateTime Value
        {
            get
            {
                if (!_value.HasValue)
                {
                    throw new InvalidOperationException("AtomDate object must have a value.");
                }
                return _value.Value;
            }
            set { _value = value; }
        }

        [XmlIgnore]
        public bool HasValue => _value.HasValue;

        /// <summary>
        /// Gets and sets the DateTime using ISO-8601 date format.
        /// For serialization purposes only, use the PubDate property instead.
        /// </summary>
        [XmlText]
        [DefaultValue(null)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string ValueIso8601
        {
            get
            {
                if (!_value.HasValue)
                {
                    return null;
                }
                return _value.Value.ToString("s")+'Z';
            }
            set
            {
                DateTime dateTime;
                if (!DateTime.TryParse(value, out dateTime))
                {
                    _value = null;
                    return;
                }

                _value = dateTime.ToUniversalTime();
            }
        }

        public DateTime GetValueOrDefault(DateTime defaultValue)
        {
            if (!_value.HasValue)
            {
                return defaultValue;
            }
            return _value.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is AtomDate)
            {
                return _value.Equals(((AtomDate)obj)._value);
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            if (!_value.HasValue)
            {
                return 0;
            }
            return _value.GetHashCode();
        }

        public static implicit operator AtomDate(DateTime value)
        {
            return new AtomDate(value);
        }

        public static explicit operator DateTime(AtomDate value)
        {
            return value.Value;
        }
    }
}