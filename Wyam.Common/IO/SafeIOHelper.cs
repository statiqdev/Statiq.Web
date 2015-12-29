using System;
using System.Collections.Generic;
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
        /// <see cref="SafeOpenRead(string)"/>
        private const int _maxAttempts = 3;

        /// <summary>
        /// Trying to open file via File.OpenRead() several times if first time was not successful, waiting 100 ms between each attempt.
        /// Often unsuccessful reads are happening while files are being watched for changes.
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>FileStream, null or base exception</returns>
        public static FileStream SafeOpenRead(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("path");

            int attempts = 0;
            FileStream file = null;

            while (attempts < _maxAttempts)
            {
                try
                {
                    attempts++;
                    file = File.OpenRead(path);
                    return file;
                }
                catch (Exception e)
                {
                    if (file != null) file.Dispose();

                    // e is DirectoryNotFoundException || e is FileNotFoundException || 
                    if (e is IOException || e is UnauthorizedAccessException)
                        System.Threading.Thread.Sleep(100);
                    else
                        throw e;
                }
            }

            // must be unreachable
            return null;
        }
    }
}
