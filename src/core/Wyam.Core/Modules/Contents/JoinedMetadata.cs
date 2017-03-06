namespace Wyam.Core.Modules.Contents
{
    /// <summary>
    /// Specifies how to deal with metadata from joined documents.
    /// </summary>
    public enum JoinedMetadata
    {
        DefaultOnly,
        FirstDocument,
        LastDocument,
        AllWithFirstDuplicates,
        AllWithLastDuplicates
    }
}