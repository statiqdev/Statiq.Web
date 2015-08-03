using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Wyam.Abstractions;
using Wyam.Core.Helpers;

namespace Wyam.Modules.ImageProcessor
{
    public class ImageProcessor : IModule
    {
        List<ImageInstruction> _instructions;

        ImageInstruction _currentInstruction;

        public ImageProcessor()
        {
            _instructions = new List<ImageInstruction>();
        }

        void EnsureCurrentInstruction()
        {
            if (_currentInstruction == null)
            {
                _currentInstruction = new ImageInstruction();
                _instructions.Add(_currentInstruction);
            }
        }

        public ImageProcessor Resize(int? width, int? height)
        {
            EnsureCurrentInstruction();

            _currentInstruction.Width = width;
            _currentInstruction.Height = height;

            return this;
        }

        public ImageProcessor ApplyFilters(params ImageFilter[] filters)
        {
            EnsureCurrentInstruction();

            _currentInstruction.Filters.AddRange(filters);

            return this;
        }

        public ImageProcessor Brighten(short percentage)
        {
            if (percentage < 0 && percentage > 100)
                throw new ArgumentException($"Percentage must be between 0 and 100 instead of {percentage}%");

            _currentInstruction.Brightness = percentage;

            return this;
        }

        public ImageProcessor Darken(short percentage)
        {
            if (percentage < 0 && percentage > 100)
                throw new ArgumentException($"Percentage must be between 0 and 100 instead of {percentage}%");

            _currentInstruction.Brightness = -percentage;

            return this;
        }

        public ImageProcessor And
        {
            get
            {
                _currentInstruction = null;
                return this;
            }
        }

        ISupportedImageFormat GetFormat(string extension)
        {
            ISupportedImageFormat format = null;

            if (extension == ".jpg" || extension == ".jpeg")
                format = new JpegFormat { Quality = 70 };
            else
                if (extension == ".gif")
                format = new GifFormat { };
            else if (extension == ".png")
                format = new PngFormat { };

            return format;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            foreach (IDocument input in inputs)
            {
                bool isBase64 = input.Get<bool>(MetadataKeys.Base64);

                if (!isBase64)
                    continue;

                var path = input.Get<string>(MetadataKeys.SourceFilePath);

                if (string.IsNullOrWhiteSpace(path))
                    continue;

                var relativePath = Path.GetDirectoryName(PathHelper.GetRelativePath(context.InputFolder, path));
                string destination = Path.Combine(context.OutputFolder, relativePath, Path.GetFileName(path));

                string destinationDirectory = Path.GetDirectoryName(destination);
                if (!Directory.Exists(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                var extension = Path.GetExtension(path);

                ISupportedImageFormat format = GetFormat(extension);
                if (format == null)
                    continue;

                byte[] photoBytes = Convert.FromBase64String(input.Content);

                foreach (var ins in _instructions)
                {
                    Size size = ins.GetSize();

                    string destinationFile = Path.GetFileNameWithoutExtension(path);
                    destinationFile += ins.GetSuffix() + extension;

                    var destinationPath = Path.Combine(destinationDirectory, destinationFile);
                    context.Trace.Verbose($"Sending processed image to {destinationPath}");

                    ProduceImage(photoBytes, format, ins, destinationPath);
                }

                yield return input;
            }
        }

        void ProduceImage(byte[] photoBytes, ISupportedImageFormat format, ImageInstruction ins, string destinationPath)
        {
            using (var inStream = new MemoryStream(photoBytes))
            {
                using (var outStream = new MemoryStream())
                {
                    using (var imageFactory = new ImageFactory(preserveExifData: true))
                    {
                        // Load, resize, set the format and quality and save an image.
                        var fac = imageFactory.Load(inStream)
                                    .Format(format);

                        if (ins.IsCropRequired)
                        {
                            var layer = new ResizeLayer(
                                size: ins.GetSize(),
                                anchorPosition: AnchorPosition.Center,
                                resizeMode: ResizeMode.Crop
                                );

                            fac.Resize(layer);
                        }
                        else
                        {
                            fac.Resize(ins.GetSize());
                        }

                        foreach (var f in ins.Filters)
                        {
                            fac.Filter(ins.GetMatrixFilter(f));
                        }

                        if (ins.Brightness.HasValue)
                        {
                            fac.Brightness(ins.Brightness.Value);
                        }

                        fac.Save(outStream);
                    }

                    outStream.Seek(0, SeekOrigin.Begin);
                    using (var f = File.Create(destinationPath))
                    {
                        outStream.CopyTo(f);
                    }
                }
            }
        }
    }
}
