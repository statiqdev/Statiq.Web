namespace Wyam.Common.Shortcodes
{
    public interface IShortcodeCollection : IReadOnlyShortcodeCollection
    {
        void Add<TShortcode>(string name)
            where TShortcode : IShortcode;
    }
}
