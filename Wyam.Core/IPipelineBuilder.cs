namespace Wyam.Core
{
    public interface IPipelineBuilder
    {
        IPipelineBuilder AddModule(IModule module);
    }
}