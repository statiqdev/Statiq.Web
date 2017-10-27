namespace Wyam.Configuration
{
    public partial class KnownExtension
    {
		public static readonly KnownExtension All = new KnownExtension("Wyam.All");
		public static readonly KnownExtension BlogTemplateTheme = new KnownExtension("Wyam.Blog.BlogTemplate");
		public static readonly KnownExtension CleanBlogTheme = new KnownExtension("Wyam.Blog.CleanBlog");
		public static readonly KnownExtension PhantomTheme = new KnownExtension("Wyam.Blog.Phantom");
		public static readonly KnownExtension SolidStateTheme = new KnownExtension("Wyam.Blog.SolidState");
		public static readonly KnownExtension StellarTheme = new KnownExtension("Wyam.Blog.Stellar");
		public static readonly KnownExtension TrophyTheme = new KnownExtension("Wyam.Blog.Trophy");
		public static readonly KnownExtension VelocityTheme = new KnownExtension("Wyam.BookSite.Velocity");
		public static readonly KnownExtension SamsonTheme = new KnownExtension("Wyam.Docs.Samson");
    }
}