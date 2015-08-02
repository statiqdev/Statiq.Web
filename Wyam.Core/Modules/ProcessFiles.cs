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

namespace Wyam.Core.Modules
{
    public class ImageInstruction
    {
        public int? Width { get; set; }

        public int? Height { get; set; }

        public ImageInstruction()
        {

        }

        public ImageInstruction(int? width, int? height)
        {
            if (!width.HasValue && !height.HasValue)
            {
                throw new ArgumentNullException($"{nameof(width)} or {nameof(height)} needs to be specified");
            }

            Width = width;
            Height = height;
        }

        public Size GetSize()
        {
            if (Width.HasValue && Height.HasValue)
                return new Size(Width.Value, Height.Value);
            else if (Width.HasValue)
                return new Size(Width.Value, 0);
            else
                return new Size(0, Height.Value);
        }
        
        public bool IsCropRequired
        {
            get
            {
                return Width.HasValue && Height.HasValue;
            }
        }

        public string GetSuffix()
        {
            string suffix = "";
            if (Width.HasValue)
                suffix += "-w" + Width.Value;

            if (Height.HasValue)
                suffix += "-h" + Height.Value;

            return suffix;
        }
    }

    public class ImageProcessor : IModule
    {
        int? _width { get; set; }

        int? _height { get; set; }

        List<ImageInstruction> _instructions;

        public ImageProcessor()
        {
            _instructions = new List<ImageInstruction>();
        }

        public ImageProcessor Resize(int? width, int? height)
        {
            var instruction = new ImageInstruction(width, height);
            _instructions.Add(instruction);

            return this;
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
            foreach (IDocument input in inputs.AsParallel())
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

                    var processedDestination = Path.Combine(destinationDirectory, destinationFile);

                    context.Trace.Verbose($"Sending processed image to {processedDestination}");

                    using (var inStream = new MemoryStream(photoBytes))
                    {
                        using (var outStream = new MemoryStream())
                        {
                            using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
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

                                fac.Save(outStream);
                            }

                            outStream.Seek(0, SeekOrigin.Begin);
                            using (var f = File.Create(processedDestination))
                            {
                                outStream.CopyTo(f);
                            }
                        }
                    }
                }

                yield return input;
            }
        }
    }

    public class ProcessFiles : IModule
    {
        private readonly Func<IDocument, string> _sourcePath;
        private Func<string, string> _destinationPath;
        private SearchOption _searchOption = System.IO.SearchOption.AllDirectories;
        private Func<string, bool> _where = null;

        public ProcessFiles(Func<IDocument, string> sourcePath)
        {
            if (sourcePath == null)
            {
                throw new ArgumentNullException("sourcePath");
            }

            _sourcePath = sourcePath;
        }

        public ProcessFiles(string searchPattern)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException("searchPattern");
            }

            _sourcePath = m => searchPattern;
        }

        public ProcessFiles SearchOption(SearchOption searchOption)
        {
            _searchOption = searchOption;
            return this;
        }

        public ProcessFiles AllDirectories()
        {
            _searchOption = System.IO.SearchOption.AllDirectories;
            return this;
        }

        public ProcessFiles TopDirectoryOnly()
        {
            _searchOption = System.IO.SearchOption.TopDirectoryOnly;
            return this;
        }

        public ProcessFiles Where(Func<string, bool> predicate)
        {
            _where = predicate;
            return this;
        }

        public ProcessFiles To(Func<string, string> destinationPath)
        {
            if (destinationPath == null)
            {
                throw new ArgumentNullException("destinationPath");
            }

            _destinationPath = destinationPath;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            var documents = from input in inputs
                            let p = _sourcePath(input)
                            where p != null
                            let path = Path.Combine(context.InputFolder, p)
                            let fileRoot = Path.GetDirectoryName(path)
                            where fileRoot != null && Directory.Exists(fileRoot)
                            select new
                            {
                                Input = input,
                                Listing = Directory.EnumerateFiles(fileRoot, Path.GetFileName(path), _searchOption).Where(x => _where == null || _where(x))
                            } into g
                            from m in g.Listing
                            let ext = Path.GetExtension(m)
                            let binary = File.ReadAllBytes(m)
                            select g.Input.Clone(Convert.ToBase64String(binary), new Dictionary<string, object>
                            {
                                [MetadataKeys.SourceFilePath] = m,
                                [MetadataKeys.SourceFileExt] = ext,
                                [MetadataKeys.Base64] = true
                            });
            
            return documents;
        }
    }
}
