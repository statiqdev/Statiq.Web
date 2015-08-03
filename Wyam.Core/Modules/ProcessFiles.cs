using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Filters.Photo;
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
    public enum ImageFilter
    {
        BlackAndWhite,
        Comic,
        Gotham,
        GreyScale,
        HiSatch,
        Invert,
        Lomograph,
        LoSatch,
        Polariod,
        Sepia,
    }

    public class ImageInstruction
    {
        public int? Width { get; set; }

        public int? Height { get; set; }

        public int? Brightness { get; set; }

        public List<ImageFilter> Filters { get; set; } = new List<ImageFilter>();

        public ImageInstruction()
        {

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

            foreach (var f in Filters)
            {
                suffix += $"-{f.ToString().ToLower()}";
            }

            if (Brightness.HasValue && Brightness > 0)
            {
                suffix += $"-b{Brightness.Value}";
            }

            if (Brightness.HasValue && Brightness < 0)
            {
                suffix += $"-d{Brightness.Value * -1}"; //only shows positive number
            }

            return suffix;
        }

        public IMatrixFilter GetMatrixFilter(ImageFilter filter)
        {
            switch (filter)
            {
                case ImageFilter.BlackAndWhite: return MatrixFilters.BlackWhite;
                case ImageFilter.Comic: return MatrixFilters.Comic;
                case ImageFilter.Gotham: return MatrixFilters.Gotham;
                case ImageFilter.GreyScale: return MatrixFilters.GreyScale;
                case ImageFilter.HiSatch: return MatrixFilters.HiSatch;
                case ImageFilter.Invert: return MatrixFilters.Invert;
                case ImageFilter.Lomograph: return MatrixFilters.Lomograph;
                case ImageFilter.LoSatch: return MatrixFilters.LoSatch;
                case ImageFilter.Polariod: return MatrixFilters.Polaroid;
                case ImageFilter.Sepia: return MatrixFilters.Sepia;
                default: return MatrixFilters.Comic;
            }
        }
       
    }

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

        public ImageProcessor ApplyFilter(ImageFilter filter)
        {
            EnsureCurrentInstruction();

            _currentInstruction.Filters.Add(filter);

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

        void ProduceImage(Byte[] photoBytes, ISupportedImageFormat format, ImageInstruction ins, string destinationPath)
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
