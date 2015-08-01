using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Abstractions;
using Wyam.Core.Helpers;

namespace Wyam.Core.Modules
{
    public class ImageInstruction
    {
        int? _width { get; set; }

        int? _height { get; set; }

        public ImageInstruction(int? width, int? height)
        {
            if (!width.HasValue && !height.HasValue)
            {
                throw new ArgumentNullException($"{nameof(width)} or {nameof(height)} needs to be specified");
            }

            _width = width;
            _height = height;
        }

        public Size GetSize()
        {
            if (_width.HasValue && _height.HasValue)
                return new Size(_width.Value, _height.Value);
            else if (_width.HasValue)
                return new Size(_width.Value, 0);
            else
                return new Size(0, _height.Value);
        }
        
        public bool IsCropRequired
        {
            get
            {
                return _width.HasValue && _height.HasValue;
            }
        }

        public string GetSuffix()
        {
            string suffix = "";
            if (_width.HasValue)
                suffix += "-w" + _width.Value;

            if (_height.HasValue)
                suffix += "-h" + _height.Value;

            return suffix;
        }
    }

    public class ImageProcessor : IModule
    {
        int? _width { get; set; }

        int? _height { get; set; }

        ImageInstruction[] _instructions;

        public ImageProcessor(params ImageInstruction[] instructions)
        {
            _instructions = instructions;
        }

        ISupportedImageFormat GetFormat(string extension)
        {
            // Format is automatically detected though can be changed.
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
                bool isHex = input.Get<bool>(MetadataKeys.Base64);

                if (!isHex)
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

                    using (MemoryStream inStream = new MemoryStream(photoBytes))
                    {
                        using (MemoryStream outStream = new MemoryStream())
                        {
                            // Initialize the ImageFactory using the overload to preserve EXIF metadata.
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

        private IModule[] _modules;

        public ProcessFiles(Func<IDocument, string> sourcePath)
        {
            if (sourcePath == null)
            {
                throw new ArgumentNullException("sourcePath");
            }

            _sourcePath = sourcePath;
        }

        public ProcessFiles(string searchPattern, params IModule[] modules)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException("searchPattern");
            }

            _sourcePath = m => searchPattern;

            _modules = modules;
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


        // Input to function is the full file path (including file name), should return a full file path (including file name)
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


            context.Trace.Verbose("Document counts " + documents.Count());

            context.Execute(_modules, documents);
            return inputs;

            //foreach (IDocument input in inputs)
            //{
            //    string path = _sourcePath(input);
            //    if (path != null)
            //    {
            //        path = Path.Combine(context.InputFolder, path);
            //        string fileRoot = Path.GetDirectoryName(path);
            //        if (fileRoot != null && Directory.Exists(fileRoot))
            //        {
            //            foreach (string file in Directory.EnumerateFiles(fileRoot, Path.GetFileName(path), _searchOption).Where(x => _where == null || _where(x)))
            //            {
            //                var extension = Path.GetExtension(file);

            //                byte[] photoBytes = File.ReadAllBytes(file);

            //                yield return input.Clone(Convert.ToBase64String(photoBytes), new Dictionary<string, object>
            //                {
            //                    {MetadataKeys.SourceFilePath, file},
            //                    {MetadataKeys.SourceFileExt, extension},
            //                    {MetadataKeys.Base64, true }
            //                });
            //            }
            //        }
            //    }
            //}

        }
    }
}
