using System;
using System.Collections.Generic;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using Wyam.Images.Operations;

namespace Wyam.Images
{
    internal class ImageOperations
    {
        public Queue<IImageOperation> Operations { get; } = new Queue<IImageOperation>();
        public List<OutputAction> OutputActions { get; } = new List<OutputAction>();

        public void Enqueue(IImageOperation operation) => Operations.Enqueue(operation);
    }
}
