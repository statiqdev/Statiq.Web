using System;

namespace Wyam.Razor
{
    internal interface IBasePageTypeProvider
    {
        Type BasePageType { get; }
    }
}