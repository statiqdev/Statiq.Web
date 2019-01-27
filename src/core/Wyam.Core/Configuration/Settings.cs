using System;
using System.Collections.Generic;
using System.Reflection;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Tracing;
using Wyam.Common.Util;
using Wyam.Core.Meta;

namespace Wyam.Core.Configuration
{
    internal class Settings : MetadataDictionary, ISettings
    {
        public Settings()
        {
            this[Common.Meta.Keys.LinkHideIndexPages] = true;
            this[Common.Meta.Keys.LinkHideExtensions] = true;
            this[Common.Meta.Keys.UseCache] = true;
            this[Common.Meta.Keys.CleanOutputPath] = true;
        }
    }
}
