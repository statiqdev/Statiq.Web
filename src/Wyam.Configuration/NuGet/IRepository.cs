namespace Wyam.NuGet
{
    internal interface IRepository
    {
        void AddPackage(string packageId, string versionSpec, bool allowPrereleaseVersions, bool allowUnlisted);
    }
}