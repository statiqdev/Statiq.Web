using img = ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Wyam.Common;
using Wyam.Core.Helpers;

namespace Wyam.Modules.ImageProcessor
{
    public class ImageProcessor : IModule
    {
        List<ImageInstruction> _instructions;

        ImageInstruction _currentInstruction;

        IModule[] _modules;

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

        public ImageProcessor ForEachOutputDocument(params IModule[] modules)
        {
            _modules = modules;
            return this; 
        }

        public ImageProcessor Resize(int? width, int? height, AnchorPosition anchor = AnchorPosition.Center)
        {
            EnsureCurrentInstruction();

            _currentInstruction.Width = width;
            _currentInstruction.Height = height;
            _currentInstruction.AnchorPosition = anchor;

            return this;
        }

        public ImageProcessor Constrain(int width, int height)
        {
            EnsureCurrentInstruction();

            _currentInstruction.Constraint = new Size(width, height);

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

                foreach (var ins in _instructions)
                {
                    if (input.Stream.CanSeek)
                    {
                        input.Stream.Seek(0, SeekOrigin.Begin);
                    }

                    string destinationFile = Path.GetFileNameWithoutExtension(path);
                    destinationFile += ins.GetSuffix() + extension;

                    var destinationPath = Path.Combine(destinationDirectory, destinationFile);
                    context.Trace.Verbose($"WritePath: {destinationPath}");

                    var output = ProcessImage(input.Stream, format, ins);

                    var clone = input.Clone(output, new Dictionary<string, object>
                        {
                            {Wyam.Core.Documents.MetadataKeys.WritePath, destinationPath},
                            {Wyam.Core.Documents.MetadataKeys.WriteExtension, extension }
                        });

                    if (_modules != null)
                    {
                        foreach(var m in _modules)
                        {
                            m.Execute(new IDocument[] { clone }, context);
                        }
                    }
                    else
                        yield return clone;
                }
            }
        }

        Stream ProcessImage(Stream inStream, ISupportedImageFormat format, ImageInstruction ins)
        {
            using (var imageFactory = new img.ImageFactory(preserveExifData: true))
            {
                // Load, resize, set the format and quality and save an image.
                var fac = imageFactory.Load(inStream)
                            .Format(format);

                if (ins.IsNeedResize)
                {
                    if (ins.IsCropRequired)
                    {
                        var layer = new ResizeLayer(
                            size: ins.GetSize().Value,
                            anchorPosition: ins.GetAnchorPosition(),
                            resizeMode: ResizeMode.Crop
                            );

                        fac.Resize(layer);
                    }
                    else
                    {
                        fac.Resize(ins.GetSize().Value);
                    }
                }

                foreach (var f in ins.Filters)
                {
                    fac.Filter(ins.GetMatrixFilter(f));
                }

                if (ins.Brightness.HasValue)
                {
                    fac.Brightness(ins.Brightness.Value);
                }

                if (ins.Constraint.HasValue)
                {
                    fac.Constrain(ins.Constraint.Value);
                }

                var outputStream = new MemoryStream();
                fac.Save(outputStream);
                outputStream.Seek(0, SeekOrigin.Begin);
                return outputStream;
            }
        }
    }
}
