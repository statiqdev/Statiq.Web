using System;
using System.IO;
using System.Text;
using System.Threading;
using Wyam.Common.Tracing;

namespace Wyam
{
    /// <summary>
    /// Reads standard input (stdin). We have to be very careful because the calling process might have
    /// opened stdin and just left it open, in which case it would register as redirected but the
    /// stream won't ever return because it's just waiting for input.
    /// </summary>
    internal static class StandardInputReader
    {
        public static string Read()
        {
            if (Console.IsInputRedirected)
            {
                Trace.Verbose("Input is redirected, attempting to read...");
                using (Stream stream = Console.OpenStandardInput())
                {
                    byte[] buffer = new byte[1000];
                    StringBuilder stdin = new StringBuilder();
                    int totalRead = 0;
                    int read = -1;
                    while (true)
                    {
                        AutoResetEvent gotInput = new AutoResetEvent(false);
                        Thread inputThread = new Thread(() =>
                        {
                            try
                            {
                                read = stream.Read(buffer, 0, buffer.Length);
                                gotInput.Set();
                            }
                            catch (ThreadAbortException)
                            {
                                Thread.ResetAbort();
                            }
                        })
                        {
                            IsBackground = true
                        };

                        inputThread.Start();

                        // Timeout expired?
                        if (!gotInput.WaitOne(100))
                        {
                            inputThread.Abort();
                            Trace.Verbose("Timeout expired while reading from input");
                            break;
                        }

                        // End of stream?
                        if (read == 0)
                        {
                            Trace.Verbose($"Read {totalRead} bytes ({stdin.Length} characters) from input");
                            return stdin.ToString();
                        }

                        // Got data
                        stdin.Append(Console.InputEncoding.GetString(buffer, 0, read));
                        totalRead += read;
                    }
                }
            }

            return null;
        }
    }
}