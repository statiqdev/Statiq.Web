using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Modules.Rss
{
    public static class DateTimeRssExtensions
    {
        private static CultureInfo rssDateCulture = new CultureInfo("en");

        public static string ToRssDate(this DateTime date)
        {
            var value = date.ToString("ddd',' d MMM yyyy HH':'mm':'ss", rssDateCulture)
                + " " + date.ToString("zzzz", rssDateCulture).Replace(":", "");
            return value;
        }
    }
}
