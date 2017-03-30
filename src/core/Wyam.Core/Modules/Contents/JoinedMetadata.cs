namespace Wyam.Core.Modules.Contents
{
    /// <summary>
    /// Specifies how to deal with metadata from joined documents.
    /// </summary>
    public enum JoinedMetadata
    {
        /// <summary>
        /// The joined document only includes the default global metadata.
        /// </summary>
        DefaultOnly,

        /// <summary>
        /// The joined document includes the metadata from the first document in the sequence of documents to join.
        /// </summary>
        FirstDocument,

        /// <summary>
        /// The joined document includes the metadata from the last document in the sequence of documents to join.
        /// </summary>
        LastDocument,

        /// <summary>
        /// The joined document includes metadata from all joined documents and uses the value from the
        /// first document in the sequence of documents to join in the case of duplicate keys.
        /// </summary>
        AllWithFirstDuplicates,

        /// <summary>
        /// The joined document includes metadata from all joined documents and uses the value from the
        /// last document in the sequence of documents to join in the case of duplicate keys.
        /// </summary>
        AllWithLastDuplicates
    }
}