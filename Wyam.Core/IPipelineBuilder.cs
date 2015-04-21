namespace Wyam.Core
{
    public interface IPipelineBuilder
    {
        IPipelineBuilder AddModule(Module module);
    }
}