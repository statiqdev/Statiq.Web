using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Wyam.Images
{
    internal static class Compressor
    {
        private static readonly ImageFormat[] SupportedFormats = { ImageFormat.Jpeg, ImageFormat.Gif, ImageFormat.Png };
        private static readonly string WorkingDirectory = Path.Combine(Path.GetDirectoryName(typeof(Compressor).Assembly.Location), "CompressorTools");

        internal static Stream Compress(Stream input, ImageFormat format, bool lossless)
        {
            if (!SupportedFormats.Contains(format))
            {
                return input;
            }

            // persist image
            string sourceTempFileName = Path.GetTempFileName();
            string targetTempFileName = Path.GetTempFileName();
            using (FileStream fileStream = File.OpenWrite(sourceTempFileName))
            using (input)
            {
                input.Seek(0, SeekOrigin.Begin);
                input.CopyTo(fileStream);
            }

            // detect tool
            string args = CreateArguments(sourceTempFileName, targetTempFileName, format, lossless);

            // execute tool
            ExecuteExternalTool(args);

            // make stream from temp file
            MemoryStream ms = new MemoryStream();
            using (FileStream fileStream = File.OpenRead(sourceTempFileName))
            {
                fileStream.Seek(0, SeekOrigin.Begin);
                fileStream.CopyTo(ms);
            }

            // delete tempfiles
            if (File.Exists(sourceTempFileName))
            {
                File.Delete(sourceTempFileName);
            }

            if (File.Exists(targetTempFileName))
            {
                File.Delete(targetTempFileName);
            }

            // return stream
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        private static string CreateArguments(string sourceFileName, string targetFileName, ImageFormat format, bool lossless)
        {
            if (format == ImageFormat.Jpeg)
            {
                if (lossless)
                {
                    return $"/c jpegtran -copy none -optimize -progressive -outfile \"{targetFileName}\" \"{sourceFileName}\"";
                }

                return $"/c cjpeg -quality 80,60 -dct float -smooth 5 -outfile \"{targetFileName}\" \"{sourceFileName}\"";
            }
            if (format == ImageFormat.Png)
            {
                if (lossless)
                {
                    return $"/c lossless_png_compression.cmd \"{sourceFileName}\" \"{targetFileName}\"";
                }

                return $"/c lossy_png_compression.cmd \"{sourceFileName}\" \"{targetFileName}\"";
            }

            if (format == ImageFormat.Gif)
            {
                return $"/c gifsicle -O3 --batch --colors=256 \"{sourceFileName}\" --output=\"{targetFileName}\"";
            }

            return null;
        }

        private static void ExecuteExternalTool(string args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("cmd")
            {
                WorkingDirectory = WorkingDirectory,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit();
            }
        }
    }
}
