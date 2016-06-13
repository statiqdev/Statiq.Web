using System;
using System.Collections.Generic;

namespace Wyam.Configuration
{
    /// <summary>
    /// Lookup data for all known recipes.
    /// </summary>
    public class KnownRecipe
    {
        public static readonly KnownRecipe Blog = new KnownRecipe("Wyam.Blog", nameof(KnownTheme.CleanBlog));

        /// <summary>
        /// The lookup for all known recipes, keyed by recipe name.
        /// </summary>
        public static readonly Dictionary<string, KnownRecipe> Lookup
            = new Dictionary<string, KnownRecipe>(StringComparer.OrdinalIgnoreCase)
            {
                {nameof(Blog), Blog}
            };

        /// <summary>
        /// Gets the package that the recipe class is in. If the recipe is in the
        /// core library, this will be null.
        /// </summary>
        /// <value>
        /// The package.
        /// </value>
        public string PackageId { get; }

        /// <summary>
        /// Gets the default theme for this recipe (or null). This should map
        /// to a theme in the <see cref="KnownTheme" /> lookup.
        /// </summary>
        /// <value>
        /// The default theme.
        /// </value>
        public string DefaultTheme { get; }

        private KnownRecipe(string packageId, string defaultTheme)
        {
            PackageId = packageId;
            DefaultTheme = defaultTheme;
        }
    }
}