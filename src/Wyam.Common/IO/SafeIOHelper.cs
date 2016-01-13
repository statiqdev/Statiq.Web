using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.IO
{
    public static class SafeIOHelper
    {
        /// <summary>
        /// Max SafeOpenRead() attempts
        /// </summary>
        /// <see cref="OpenRead"/>
        private const int MaxAttempts = 3;

        /// <summary>
        /// Trying to open file via File.OpenRead() several times if first time was not successful, waiting 100 ms between each attempt.
        /// Often unsuccessful reads are happening while files are being watched for changes.
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>FileStream, null or base exception</returns>
        public static FileStream OpenRead(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("path");
            }

            int attempts = 0;
            while (true)
            {
                try
                {
                    attempts++;
                    return File.OpenRead(path);
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

        /// <summary>
        /// Reads the specified file with retry logic.
        /// </summary>
        /// <param name="path">Path to the file to read.</param>
        public static string ReadAllText(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("path");
            }

            int attempts = 0;
            while (true)
            {
                try
                {
                    attempts++;
                    return File.ReadAllText(path);
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

        /// <summary>
        /// Copies the specified file with retry logic.
        /// </summary>
        /// <param name="sourceFileName">Path of the source file.</param>
        /// <param name="destFileName">Path of the destination file.</param>
        /// <param name="overwrite">If set to <c>true</c>, overwrites an existing file.</param>
        public static void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            int attempts = 0;
            while (true)
            {
                try
                {
                    attempts++;
                    File.Copy(sourceFileName, destFileName, overwrite);
                    return;
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
