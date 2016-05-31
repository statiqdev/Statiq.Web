namespace Wyam.Modules.Git
{
    public static class GitKeys
    {
        // Commits
        public const string Sha = nameof(Sha); // string
        public const string Parents = nameof(Parents); // IReadOnlyList<string>, SHA of the parent(s)
        public const string AuthorName = nameof(AuthorName); // string
        public const string AuthorEmail = nameof(AuthorEmail); // string
        public const string AuthorWhen = nameof(AuthorWhen); // DateTimeOffset
        public const string CommitterName = nameof(CommitterName); // string
        public const string CommitterEmail = nameof(CommitterEmail); // string
        public const string CommitterWhen = nameof(CommitterWhen); // DateTimeOffset
        public const string Message = nameof(Message); // string
        public const string Entries = nameof(Entries); // IReadOnlyDictionary<string, string>, key = path, value = status
        public const string Commits = nameof(Commits); // IReadOnlyList<IDocument>
        public const string ContributorName = nameof(ContributorName); // string
        public const string ContributorEmail = nameof(ContributorEmail); // string
        public const string Contributors = nameof(Contributors); // IReadOnlyList<IDocument>
    }
}