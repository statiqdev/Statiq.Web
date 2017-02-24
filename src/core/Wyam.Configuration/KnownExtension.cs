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
    }
}
