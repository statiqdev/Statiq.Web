using System;
using System.Collections.Generic;
using System.Reflection;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Tracing;
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

        [Obsolete]
        public string Host
        {
            get
            {
                Trace.Warning("Settings.Host is deprecated and will be removed in a future version. Please use Settings[Keys.Host] instead.");
                return String(Common.Meta.Keys.Host);
            }
            set
            {
                Trace.Warning("Settings.Host is deprecated and will be removed in a future version. Please use Settings[Keys.Host] instead.");
                this[Common.Meta.Keys.Host] = value;
            }
        }

        [Obsolete]
        public bool LinksUseHttps
        {
            get
            {
                Trace.Warning("Settings.LinksUseHttps is deprecated and will be removed in a future version. Please use Settings[Keys.LinksUseHttps] instead.");
                return Get<bool>(Common.Meta.Keys.LinksUseHttps);
            }
            set
            {
                Trace.Warning("Settings.LinksUseHttps is deprecated and will be removed in a future version. Please use Settings[Keys.LinksUseHttps] instead.");
                this[Common.Meta.Keys.LinksUseHttps] = value;
            }
        }

        [Obsolete]
        public DirectoryPath LinkRoot
        {
            get
            {
                Trace.Warning("Settings.LinkRoot is deprecated and will be removed in a future version. Please use Settings[Keys.LinkRoot] instead.");
                return DirectoryPath(Common.Meta.Keys.LinkRoot);
            }
            set
            {
                Trace.Warning("Settings.LinkRoot is deprecated and will be removed in a future version. Please use Settings[Keys.LinkRoot] instead.");
                this[Common.Meta.Keys.LinkRoot] = value;
            }
        }

        [Obsolete]
        public bool LinkHideIndexPages
        {
            get
            {
                Trace.Warning("Settings.LinkHideIndexPages is deprecated and will be removed in a future version. Please use Settings[Keys.LinkHideIndexPages] instead.");
                return Get<bool>(Common.Meta.Keys.LinkHideIndexPages);
            }
            set
            {
                Trace.Warning("Settings.LinkHideIndexPages is deprecated and will be removed in a future version. Please use Settings[Keys.LinkHideIndexPages] instead.");
                this[Common.Meta.Keys.LinkHideIndexPages] = value;
            }
        }

        [Obsolete]
        public bool LinkHideExtensions
        {
            get
            {
                Trace.Warning("Settings.LinkHideExtensions is deprecated and will be removed in a future version. Please use Settings[Keys.LinkHideExtensions] instead.");
                return Get<bool>(Common.Meta.Keys.LinkHideExtensions);
            }
            set
            {
                Trace.Warning("Settings.LinkHideExtensions is deprecated and will be removed in a future version. Please use Settings[Keys.LinkHideExtensions] instead.");
                this[Common.Meta.Keys.LinkHideExtensions] = value;
            }
        }

        [Obsolete]
        public bool UseCache
        {
            get
            {
                Trace.Warning("Settings.UseCache is deprecated and will be removed in a future version. Please use Settings[Keys.UseCache] instead.");
                return Get<bool>(Common.Meta.Keys.UseCache);
            }
            set
            {
                Trace.Warning("Settings.UseCache is deprecated and will be removed in a future version. Please use Settings[Keys.UseCache] instead.");
                this[Common.Meta.Keys.UseCache] = value;
            }
        }

        [Obsolete]
        public bool CleanOutputPath
        {
            get
            {
                Trace.Warning("Settings.CleanOutputPath is deprecated and will be removed in a future version. Please use Settings[Keys.CleanOutputPath] instead.");
                return Get<bool>(Common.Meta.Keys.CleanOutputPath);
            }
            set
            {
                Trace.Warning("Settings.CleanOutputPath is deprecated and will be removed in a future version. Please use Settings[Keys.CleanOutputPath] instead.");
                this[Common.Meta.Keys.CleanOutputPath] = value;
            }
        }
    }
}
