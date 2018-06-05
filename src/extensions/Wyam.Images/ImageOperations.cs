using System.Collections.Generic;
using SixLabors.ImageSharp.Formats;
using Wyam.Images.Operations;

namespace Wyam.Images
{
    internal class ImageOperations
    {
        public Queue<IImageOperation> Operations { get; } = new Queue<IImageOperation>();
        public List<(IImageFormat, IImageEncoder)> Formats { get; } = new List<(IImageFormat, IImageEncoder)>();

        public void Enqueue(IImageOperation operation) => Operations.Enqueue(operation);
    }
}
