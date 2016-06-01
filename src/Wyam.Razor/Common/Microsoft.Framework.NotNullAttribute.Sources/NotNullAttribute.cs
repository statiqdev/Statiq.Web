// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// This is just included in Wyam.Razor to support the uses throughout the MVC code and make differencing easier

using System;

namespace Wyam.Razor.Microsoft.Framework.Internal
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    internal sealed class NotNullAttribute : Attribute
    {
    }
}