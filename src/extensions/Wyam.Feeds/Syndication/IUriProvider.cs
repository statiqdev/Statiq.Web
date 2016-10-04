using System;

namespace Wyam.Feeds.Syndication
{
    public interface IUriProvider
    {
        Uri Uri { get; }
    }
}