using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.FileProviders;

namespace Wyam.Modules.Razor
{
    public static class FileInfoExtensions
    {
        private const int MaxAttempts = 3;

        public static Stream CreateReadStreamWithRetry(this IFileInfo fileInfo)
        {
            if (fileInfo == null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            int attempts = 0;
            while (true)
            {
                try
                {
                    attempts++;
                    return fileInfo.CreateReadStream();
                }
                catch (Exception e)
                {
                    if (attempts < MaxAttempts && (e is IOException || e is UnauthorizedAccessException))
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}
