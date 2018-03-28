using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ConcurrentCollections;
using Wyam.Common.Configuration;
using Wyam.Common.IO;
using Wyam.Common.Tracing;
using Wyam.Common.Util;
using Wyam.Configuration.ConfigScript;
using Wyam.Core.Configuration;
using Wyam.Core.Execution;

namespace Wyam.Configuration.Assemblies
{
    /// <summary>
    /// Compares two assemblies for equality by comparing at their full names.
    /// </summary>
    public class AssemblyComparer : IEqualityComparer<Assembly>
    {
        /// <inheritdoc/>
        public bool Equals(Assembly x, Assembly y) => x?.FullName.Equals(y?.FullName) ?? false;

        /// <inheritdoc/>
        public int GetHashCode(Assembly obj) => obj?.GetHashCode() ?? 0;
    }
}