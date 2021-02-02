using System;
using System.Globalization;
using System.Threading.Tasks;
using Statiq.App;
using Statiq.Web;

namespace Statiq.Web.Examples
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            // Ensure consistent date handling
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

            return await Bootstrapper
                .Factory
                .CreateWeb(args)
                .RunAsync();
        }
    }
}
