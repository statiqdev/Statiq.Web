// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using Microsoft.AspNet.FileProviders;
using Wyam.Modules.Razor.Microsoft.AspNet.Mvc.Razor.Internal;
using Wyam.Modules.Razor.Microsoft.Framework.Internal;

namespace Wyam.Modules.Razor.Microsoft.AspNet.Mvc.Razor.Compilation
{
    public static class RazorFileHash
    {
        public static string GetHash([NotNull] IFileInfo file)
        {
            try
            {
                using (var stream = file.CreateReadStream())
                {
                    return Crc32.Calculate(stream).ToString(CultureInfo.InvariantCulture);
                }
            }
            catch (IOException)
            {
                // Don't throw if reading the file fails.
            }

            return string.Empty;
        }
    }
}