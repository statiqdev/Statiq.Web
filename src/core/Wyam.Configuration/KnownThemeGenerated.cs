namespace Wyam.Configuration
{
    public partial class KnownTheme
    {
		public static readonly KnownTheme BlogTemplate = new KnownTheme(nameof(KnownRecipe.Blog), null, "Wyam.Blog.BlogTemplate");
		public static readonly KnownTheme CleanBlog = new KnownTheme(nameof(KnownRecipe.Blog), null, "Wyam.Blog.CleanBlog");
		public static readonly KnownTheme Phantom = new KnownTheme(nameof(KnownRecipe.Blog), null, "Wyam.Blog.Phantom");
		public static readonly KnownTheme SolidState = new KnownTheme(nameof(KnownRecipe.Blog), null, "Wyam.Blog.SolidState");
		public static readonly KnownTheme Stellar = new KnownTheme(nameof(KnownRecipe.Blog), null, "Wyam.Blog.Stellar");
		public static readonly KnownTheme Trophy = new KnownTheme(nameof(KnownRecipe.Blog), null, "Wyam.Blog.Trophy");
		public static readonly KnownTheme Velocity = new KnownTheme(nameof(KnownRecipe.BookSite), null, "Wyam.BookSite.Velocity");
		public static readonly KnownTheme Samson = new KnownTheme(nameof(KnownRecipe.Docs), null, "Wyam.Docs.Samson");
    }
}