namespace Wyam.Configuration.Preprocessing
{
    internal interface IPreprocessor
    {
        bool ContainsDirective(string name);
    }
}
