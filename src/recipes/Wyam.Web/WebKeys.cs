using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.IO;

namespace Wyam.Web
{
    /// <summary>
    /// Metadata keys for common web pipelines.
    /// </summary>
    public static class WebKeys
    {
        /// <summary>
        /// Set by the system for documents that support editing. Contains the
        /// relative path to the document to be appended to the base edit URL.
        /// </summary>
        /// <type><see cref="FilePath"/></type>
        public const string EditFilePath = nameof(EditFilePath);
    }
}
