namespace Wyam.Configuration
{
    /// <summary>
     /// Lookup data for all known extensions.
     /// </summary>
    public partial class KnownExtension : ClassEnum<KnownExtension>
    {
        // Third-party extension field declarations would go here

        /// <summary>
        /// Gets the package that contains the extension.
        /// </summary>
        public string PackageId { get; }

        private KnownExtension(string packageId)
        {
            PackageId = packageId;
        }

        public static readonly KnownExtension All = new KnownExtension("Wyam.All");

        public static readonly KnownExtension AmazonWebServices = new KnownExtension("Wyam.AmazonWebServices");
        public static readonly KnownExtension CodeAnalysis = new KnownExtension("Wyam.CodeAnalysis");
        public static readonly KnownExtension Feeds = new KnownExtension("Wyam.Feeds");
        public static readonly KnownExtension GitHub = new KnownExtension("Wyam.GitHub");
        public static readonly KnownExtension Highlight = new KnownExtension("Wyam.Highlight");
        public static readonly KnownExtension Html = new KnownExtension("Wyam.Html");
        public static readonly KnownExtension Images = new KnownExtension("Wyam.Images");
        public static readonly KnownExtension Json = new KnownExtension("Wyam.Json");
        public static readonly KnownExtension Less = new KnownExtension("Wyam.Less");
        public static readonly KnownExtension Markdown = new KnownExtension("Wyam.Markdown");
        public static readonly KnownExtension Minification = new KnownExtension("Wyam.Minification");
        public static readonly KnownExtension Razor = new KnownExtension("Wyam.Razor");
        public static readonly KnownExtension Sass = new KnownExtension("Wyam.Sass");
        public static readonly KnownExtension SearchIndex = new KnownExtension("Wyam.SearchIndex");
        public static readonly KnownExtension Tables = new KnownExtension("Wyam.Tables");
        public static readonly KnownExtension TextGeneration = new KnownExtension("Wyam.TextGeneration");
        public static readonly KnownExtension Xmp = new KnownExtension("Wyam.Xmp");
        public static readonly KnownExtension Yaml = new KnownExtension("Wyam.Yaml");
        public static readonly KnownExtension YouTube = new KnownExtension("Wyam.YouTube");

        public static readonly KnownExtension BlogTemplateTheme = new KnownExtension("Wyam.Blog.BlogTemplate");
        public static readonly KnownExtension CleanBlogTheme = new KnownExtension("Wyam.Blog.CleanBlog");
        public static readonly KnownExtension PhantomTheme = new KnownExtension("Wyam.Blog.Phantom");
        public static readonly KnownExtension SolidStateTheme = new KnownExtension("Wyam.Blog.SolidState");
        public static readonly KnownExtension StellarTheme = new KnownExtension("Wyam.Blog.Stellar");
        public static readonly KnownExtension TrophyTheme = new KnownExtension("Wyam.Blog.Trophy");
        public static readonly KnownExtension SamsonTheme = new KnownExtension("Wyam.Docs.Samson");
    }
}
