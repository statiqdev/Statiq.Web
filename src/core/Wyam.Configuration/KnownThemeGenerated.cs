namespace Wyam.Configuration
{
    public partial class KnownTheme
    {
		public static readonly KnownTheme CleanBlog = new KnownTheme(nameof(KnownRecipe.Blog), null, new[] { "Wyam.Blog.CleanBlog" });
		public static readonly KnownTheme Phantom = new KnownTheme(nameof(KnownRecipe.Blog), null, new[] { "Wyam.Blog.Phantom" });
    }
}