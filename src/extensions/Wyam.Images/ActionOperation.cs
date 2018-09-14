using System;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Wyam.Common.IO;

namespace Wyam.Images.Operations
{
    internal class ActionOperation : IImageOperation
    {
        private readonly Func<IImageProcessingContext<Rgba32>, IImageProcessingContext<Rgba32>> _operation;
        private readonly Func<FilePath, FilePath> _pathModifier;

        public ActionOperation(
            Func<IImageProcessingContext<Rgba32>, IImageProcessingContext<Rgba32>> operation,
            Func<FilePath, FilePath> pathModifier)
        {
            _operation = operation;
            _pathModifier = pathModifier;
        }

        public IImageProcessingContext<Rgba32> Apply(IImageProcessingContext<Rgba32> image) =>
            _operation == null ? image : _operation(image);

        public FilePath GetPath(FilePath path) =>
            _pathModifier == null ? path : _pathModifier(path);
    }
}
