using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Wyam.Hosting.Middleware;

using Wyam.Hosting.Middleware;

namespace Wyam.Hosting
{
    internal class PreviewServerOptions
    {
        public string LocalPath { get; set; }
    }
}