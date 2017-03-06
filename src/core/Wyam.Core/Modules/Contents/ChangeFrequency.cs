namespace Wyam.Core.Modules.Contents
{
    /// <summary>
    /// The change frequency for each item in the site map.
    /// </summary>
    public enum ChangeFrequency
    {
        Always = 0,
        Hourly,
        Daily,
        Weekly,
        Monthly,
        Yearly,
        Never
    }
}