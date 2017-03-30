namespace Wyam.Core.Modules.Contents
{
    /// <summary>
    /// The change frequency for each item in the site map.
    /// </summary>
    public enum ChangeFrequency
    {
        /// <summary>
        /// The item always changes.
        /// </summary>
        Always = 0,

        /// <summary>
        /// The item changes hourly.
        /// </summary>
        Hourly,

        /// <summary>
        /// The item changes daily.
        /// </summary>
        Daily,

        /// <summary>
        /// The item changes weekly.
        /// </summary>
        Weekly,

        /// <summary>
        /// The item changes monthly.
        /// </summary>
        Monthly,

        /// <summary>
        /// The item changes yearly.
        /// </summary>
        Yearly,

        /// <summary>
        /// The item never changes.
        /// </summary>
        Never
    }
}