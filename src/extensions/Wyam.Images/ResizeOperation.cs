using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;
using Wyam.Common.IO;

namespace Wyam.Images.Operations
{
    internal class ResizeOperation : IImageOperation
    {
        private readonly int? _width;
        private readonly int? _height;
        private readonly AnchorPositionMode _anchor;
        private readonly ResizeMode _mode;

        public ResizeOperation(int? width, int? height, AnchorPositionMode anchor, ResizeMode mode)
        {
            _width = width;
            _height = height;
            _mode = mode;
            _anchor = anchor;
        }

        public IImageProcessingContext<Rgba32> Apply(IImageProcessingContext<Rgba32> image)
        {
            Size? size = GetSize();
            if (size == null)
            {
                return image;
            }
            return image.Resize(new ResizeOptions
            {
                Size = size.Value,
                Position = _anchor,
                Mode = _mode
            });
        }

        public FilePath GetPath(FilePath path)
        {
            if (_width.HasValue && _height.HasValue)
            {
                return path.InsertSuffix($"-w{_width.Value}-h{_height.Value}");
            }
            else if (_width.HasValue)
            {
                return path.InsertSuffix($"-w{_width.Value}");
            }
            else if (_height.HasValue)
            {
                return path.InsertSuffix($"-h{_height.Value}");
            }
            return path;
        }

        private Size? GetSize()
        {
            if (_width.HasValue && _height.HasValue)
            {
                return new Size(_width.Value, _height.Value);
            }
            else if (_width.HasValue)
            {
                return new Size(_width.Value, 0);
            }
            else if (_height.HasValue)
            {
                return new Size(0, _height.Value);
            }

            return null;
        }
    }
}
