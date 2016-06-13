using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Wyam.Configuration
{
    /// <summary>
    /// Lookup data for all known themes.
    /// </summary>
    public class KnownTheme
    {
        public static readonly KnownTheme None = new KnownTheme(null, null, null);
        public static readonly KnownTheme CleanBlog = new KnownTheme(nameof(KnownRecipe.Blog), null, new [] {"Wyam.Blog.CleanBlog"});

        /// <summary>
        /// The lookup of all known themes, keyed by theme name.
        /// </summary>
        public static readonly Dictionary<string, KnownTheme> Lookup
            = new Dictionary<string, KnownTheme>(StringComparer.OrdinalIgnoreCase)
            {
                {nameof(None), None },  // This allows overriding a default theme by specifying the "none" theme
                {nameof(CleanBlog), CleanBlog}
            };

        /// <summary>
        /// Gets the recipe that this theme supports. A null value indicates the theme
        /// is not recipe specific.
        /// </summary>
        /// <value>
        /// The recipe.
        /// </value>
        public string Recipe { get; }

        /// <summary>
        /// Path to insert into input paths. If the theme is just a NuGet content package,
        /// the content folder will be automatically included and this can be null.
        /// </summary>
        /// <value>
        /// The input path.
        /// </value>
        public string InputPath { get; }

        /// <summary>
        /// Gets the packages needed for this theme (content package, file provider packages, etc.).
        /// If the theme uses a non-core file provider for the provided path, the NuGet package(s)
        /// containing the provider(s) should be in this value.
        /// </summary>
        /// <value>
        /// The packages.
        /// </value>
        public string[] PackageIds { get; }

        private KnownTheme(string recipe, string inputPath, string[] packageIds)
        {
            Recipe = recipe;
            InputPath = inputPath;
            PackageIds = packageIds;
        }
    }
}