namespace Wyam.Configuration
{
    /// <summary>
    /// Lookup data for all known themes.
    /// </summary>
    public partial class KnownTheme : ClassEnum<KnownTheme>
    {
        // Third-party theme field declarations would go here

        // Allows overriding a default theme for a recipe (I.e., set the theme to "none")
        public static readonly KnownTheme None = new KnownTheme(null, null, null);

        /// <summary>
        /// Gets the recipe that this theme supports. A null value indicates the theme
        /// is not recipe specific.
        /// </summary>
        public string Recipe { get; }

        /// <summary>
        /// Path to insert into input paths. If the theme is just a NuGet content package,
        /// the content folder will be automatically included and this can be null. Useful
        /// if the theme exists somewhere else like a GitHub repository.
        /// </summary>
        public string InputPath { get; }

        /// <summary>
        /// Gets the package containing this theme.
        /// </summary>
        public string PackageId { get; }

        private KnownTheme(string recipe, string inputPath, string packageId)
        {
            Recipe = recipe;
            InputPath = inputPath;
            PackageId = packageId;
        }

        public static readonly KnownTheme BlogTemplate = new KnownTheme(nameof(KnownRecipe.Blog), null, "Wyam.Blog.BlogTemplate");
        public static readonly KnownTheme CleanBlog = new KnownTheme(nameof(KnownRecipe.Blog), null, "Wyam.Blog.CleanBlog");
        public static readonly KnownTheme Phantom = new KnownTheme(nameof(KnownRecipe.Blog), null, "Wyam.Blog.Phantom");
        public static readonly KnownTheme SolidState = new KnownTheme(nameof(KnownRecipe.Blog), null, "Wyam.Blog.SolidState");
        public static readonly KnownTheme Stellar = new KnownTheme(nameof(KnownRecipe.Blog), null, "Wyam.Blog.Stellar");
        public static readonly KnownTheme Trophy = new KnownTheme(nameof(KnownRecipe.Blog), null, "Wyam.Blog.Trophy");
        public static readonly KnownTheme Samson = new KnownTheme(nameof(KnownRecipe.Docs), null, "Wyam.Docs.Samson");
    }
}