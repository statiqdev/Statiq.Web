namespace Wyam.Common.IO
{
    /// <summary>
    /// The kind of the path.
    /// </summary>
    public enum PathKind
    {
        /// <summary>
        /// The path is absolute.
        /// </summary>
        Absolute,

        /// <summary>
        /// The path is relative.
        /// </summary>
        Relative,

        /// <summary>
        /// The path can be either relative or absolute.
        /// </summary>
        RelativeOrAbsolute
    }
}