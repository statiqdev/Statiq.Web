using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Wyam.Feeds.Syndication.Rss
{
    [Serializable]
    public struct RssDate
    {
        private DateTime? _value;

        public RssDate(DateTime date)
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
                    throw new InvalidOperationException("RssDate object must have a value.");
                }
                return _value.Value;
            }

            set
            {
                _value = value;
            }
        }

        [XmlIgnore]
        public bool HasValue => _value.HasValue;

        /// <summary>
        /// Gets and sets the DateTime using RFC-822 date format.
        /// For serialization purposes only, use the PubDate property instead.
        /// </summary>
        [XmlText]
        [DefaultValue(null)]
        public string ValueRfc822
        {
            get
            {
                if (!_value.HasValue)
                {
                    return null;
                }

                return _value.Value.ToString("R");
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
            if (obj is RssDate)
            {
                return _value.Equals(((RssDate)obj)._value);
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

        public static implicit operator RssDate(DateTime value)
        {
            return new RssDate(value);
        }

        public static explicit operator DateTime(RssDate value)
        {
            return value.Value;
        }
    }
}