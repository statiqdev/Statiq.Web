namespace Wyam.Abstractions
{
    public interface INamespacesCollection
    {
        INamespacesCollection Using(string @namespace);
    }
}