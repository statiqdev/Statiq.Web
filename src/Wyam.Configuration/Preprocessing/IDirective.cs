namespace Wyam.Configuration.Preprocessing
{
    internal interface IDirective
    {
        void Process(string value);
    }
}
