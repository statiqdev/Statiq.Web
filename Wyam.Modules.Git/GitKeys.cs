namespace Wyam.Modules.Git
{
    public static class GitKeys
    {
        // Commits
        public const string Sha = "Sha"; // string
        public const string Parents = "Parents"; // IReadOnlyList<string>, SHA of the parent(s)
        public const string AuthorName = "AuthorName"; // string
        public const string AuthorEmail = "AuthorEmail"; // string
        public const string AuthorWhen = "AuthorWhen"; // DateTimeOffset
        public const string CommitterName = "CommitterName"; // string
        public const string CommitterEmail = "CommitterEmail"; // string
        public const string CommitterWhen = "CommitterWhen"; // DateTimeOffset
        public const string Message = "Message"; // string
        public const string Entries = "Entries"; // IReadOnlyDictionary<string, string>, key = path, value = status
        public const string Commits = "Commits"; // IReadOnlyList<IDocument>
        public const string ContributorName = "ContributorName"; // string
        public const string ContributorEmail = "ContributorEmail"; // string
        public const string Contributors = "Contributors"; // IReadOnlyList<IDocument>
    }
}