namespace Wyam.Abstractions
{
    public interface IPipelineCollection
    {
        IPipeline Add(params IModule[] modules);
        IPipeline Add(string name, params IModule[] modules);
    }
}
