using System;
using System.Globalization;
using Wyam.Common.Meta;

namespace Wyam.Common.Execution
{
    /// <summary>
    /// Extensions for working with input and output date cultures.
    /// </summary>
    public static class DateTimeCultureExtensions
    {
        /// <summary>
        /// Attempts to parse and input date using the input date culture setting.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="s">The string to parse.</param>
        /// <param name="result">The resulting <see cref="DateTime"/> instance.</param>
        /// <returns><c>true</c> if the input string could be parsed, <c>false</c> otherwise.</returns>
        public static bool TryParseInputDateTime(this IExecutionContext context, string s, out DateTime result) =>
            DateTime.TryParse(s, context.GetDateTimeInputCulture(), DateTimeStyles.None, out result);

        /// <summary>
        /// Gets the <see cref="CultureInfo"/> for the date input culture.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <returns>The date input culture.</returns>
        public static CultureInfo GetDateTimeInputCulture(this IExecutionContext context)
        {
            if (!context.ContainsKey(Keys.DateTimeInputCulture))
            {
                return CultureInfo.CurrentCulture;
            }
            object value = context.Get(Keys.DateTimeInputCulture);
            return value as CultureInfo ?? CultureInfo.GetCultureInfo(value.ToString());
        }

        /// <summary>
        /// Gets a short date display string using the date display culture setting.
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime"/> to generate a string for.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="targetCulture">The culture that should be used if the date display setting isn't provided. If the
        /// current culture is of the same family, then it will be used. If not, the specified target culture will be used.</param>
        /// <returns>A short date display string.</returns>
        public static string ToShortDateString(this DateTime dateTime, IExecutionContext context, string targetCulture = "en-GB") => 
            dateTime.ToString(context.GetDateTimeDisplayCulture(targetCulture).DateTimeFormat.ShortDatePattern);

        /// <summary>
        /// Gets a long date display string using the date display culture setting.
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime"/> to generate a string for.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="targetCulture">The culture that should be used if the date display setting isn't provided. If the
        /// current culture is of the same family, then it will be used. If not, the specified target culture will be used.</param>
        /// <returns>A long date display string.</returns>
        public static string ToLongDateString(this DateTime dateTime, IExecutionContext context, string targetCulture = "en-GB") =>
            dateTime.ToString(context.GetDateTimeDisplayCulture(targetCulture).DateTimeFormat.LongDatePattern);

        /// <summary>
        /// Gets the <see cref="CultureInfo"/> for the date display culture.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="targetCulture">The culture that should be used if the date display setting isn't provided. If the
        /// current culture is of the same family, then it will be used. If not, the specified target culture will be used.</param>
        /// <returns>The date display culture.</returns>
        public static CultureInfo GetDateTimeDisplayCulture(this IExecutionContext context, string targetCulture = "en-GB")
        {
            if (!context.ContainsKey(Keys.DateTimeDisplayCulture))
            {
                CultureInfo target = CultureInfo.GetCultureInfo(targetCulture);
                return CultureInfo.CurrentCulture.TwoLetterISOLanguageName.Equals(target.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase)
                    ? CultureInfo.CurrentCulture : target;
            }
            object value = context.Get(Keys.DateTimeDisplayCulture);
            return value as CultureInfo ?? CultureInfo.GetCultureInfo(value.ToString());
        }
    }
}
