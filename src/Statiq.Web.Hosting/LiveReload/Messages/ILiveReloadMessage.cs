namespace Statiq.Web.Hosting.LiveReload.Messages
{
    internal interface ILiveReloadMessage
    {
        string Command { get; set; }
    }
}