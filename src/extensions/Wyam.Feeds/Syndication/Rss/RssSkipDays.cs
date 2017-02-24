using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml.Serialization;

namespace Wyam.Feeds.Syndication.Rss
{
    /// <summary>
    /// RSS 2.0 SkipDays
    ///     http://blogs.law.harvard.edu/tech/skipHoursDays
    /// </summary>
    [Serializable]
    public class RssSkipDays : RssBase
    {
        private static readonly int[] DayMasks = new int[7];
        private const int EmptyDays = 0x0;

        private BitVector32 _days = new BitVector32(EmptyDays);

        static RssSkipDays()
        {
            int i = (int)DayOfWeek.Sunday;
            DayMasks[i] = BitVector32.CreateMask(0);
            for (i++; i <= (int)DayOfWeek.Saturday; i++)
            {
                DayMasks[i] = BitVector32.CreateMask(DayMasks[i - 1]);
            }
        }

        public RssSkipDays() { }

        [XmlIgnore]
        public bool this[DayOfWeek day]
        {
            get { return _days[DayMasks[(int)day]]; }
            set { _days[DayMasks[(int)day]] = value; }
        }

        [XmlElement("day")]
        public string[] Days
        {
            get
            {
                List<string> skipped = new List<string>();

                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    if (this[day])
                    {
                        skipped.Add(day.ToString("G"));
                    }
                }

                return skipped.ToArray();
            }
            set
            {
                _days = new BitVector32(EmptyDays);
                if (value == null)
                {
                    return;
                }

                foreach (string day in value)
                {
                    try
                    {
                        this[(DayOfWeek)Enum.Parse(typeof(DayOfWeek), day, true)] = true;
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }

        public bool IsEmpty()
        {
            return _days.Data == EmptyDays;
        }
    }
}