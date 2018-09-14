using System;
using System.Collections.Generic;
using System.Text;

namespace Wyam.Handlebars.Tests
{
    public static class StringExtensions
    {
        public static string NoRN(this string value) => value.Replace("\r", string.Empty).Replace("\n", string.Empty);
    }
}
