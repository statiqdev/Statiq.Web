namespace Wyam.Configuration
{
    /// <summary>
    /// Lookup data for all known recipes.
    /// </summary>
    public partial class KnownRecipe : ClassEnum<KnownRecipe>
    {
        public static readonly KnownRecipe Blog = new KnownRecipe("Wyam.Blog", nameof(KnownTheme.CleanBlog));
        public static readonly KnownRecipe Docs = new KnownRecipe("Wyam.Docs", nameof(KnownTheme.Samson));

        /// <summary>
        /// Gets the package that the recipe class is in. If the recipe is in the
        /// core library, this will be null.
        /// </summary>
        public string PackageId { get; }

        /// <summary>
        /// Gets the default theme for this recipe (or null). This should map
        /// to a theme in the <see cref="KnownTheme" /> lookup.
        /// </summary>
        public string DefaultTheme { get; }

        private KnownRecipe(string packageId, string defaultTheme)
        {
            PackageId = packageId;
            DefaultTheme = defaultTheme;
        }
    }
}