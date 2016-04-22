namespace Wyam.Core.Configuration.Preprocessing
{
    internal interface IPreprocessor
    {
        bool ContainsDirective(string name);
    }
}
