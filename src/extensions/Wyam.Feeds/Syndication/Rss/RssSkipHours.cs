using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml.Serialization;

namespace Wyam.Feeds.Syndication.Rss
{
    /// <summary>
    /// RSS 2.0 SkipHours
    ///		http://blogs.law.harvard.edu/tech/skipHoursDays
    /// </summary>
    [Serializable]
    public class RssSkipHours : RssBase
    {
        private static readonly int[] HourMasks = new int[24];
        private const int EmptyHours = 0x0;
        private const int MinHour = 0;
        private const int MaxHour = 23;

        private BitVector32 _hours = new BitVector32(EmptyHours);

        static RssSkipHours()
        {
            int i = MinHour;
            HourMasks[i] = BitVector32.CreateMask(0);
            for (i++; i<=MaxHour; i++)
            {
                HourMasks[i] = BitVector32.CreateMask(HourMasks[i-1]);
            }
        }

        [XmlIgnore]
        public bool this[int hour]
        {
            get
            {
                if (hour < MinHour || hour > MaxHour)
                {
                    return false;
                }

                return _hours[HourMasks[hour]];
            }
            set
            {
                if (hour < MinHour || hour > MaxHour)
                {
                    return;
                }

                _hours[HourMasks[hour]] = value;
            }
        }

        [XmlElement("hour")]
        public int[] Hours
        {
            get
            {
                List<int> skipped = new List<int>();

                for (int i=MinHour; i<=MaxHour; i++)
                {
                    if (this[i])
                        skipped.Add(i);
                }

                return skipped.ToArray();
            }
            set
            {
                _hours = new BitVector32(EmptyHours);
                if (value == null)
                {
                    return;
                }

                foreach (int i in value)
                {
                    this[i] = true;
                }
            }
        }

        public bool IsEmpty()
        {
            return _hours.Data == EmptyHours;
        }
    }
}