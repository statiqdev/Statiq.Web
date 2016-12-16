namespace Wyam.Configuration
{
    public partial class KnownTheme
    {
		public static readonly KnownTheme CleanBlog = new KnownTheme(nameof(KnownRecipe.Blog), null, new[] { "Wyam.Blog.CleanBlog" });
		public static readonly KnownTheme Phantom = new KnownTheme(nameof(KnownRecipe.Blog), null, new[] { "Wyam.Blog.Phantom" });
		public static readonly KnownTheme Samson = new KnownTheme(nameof(KnownRecipe.Docs), null, new[] { "Wyam.Docs.Samson" });
    }
}