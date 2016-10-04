using System;

namespace Wyam.Razor
{
    internal class BasePageTypeProvider : IBasePageTypeProvider
    {
        public BasePageTypeProvider(Type basePageType)
        {
            BasePageType = basePageType;
        }

        public Type BasePageType { get; }
    }
}