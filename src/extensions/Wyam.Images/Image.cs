using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Filters;
using SixLabors.ImageSharp.Processing.Overlays;
using SixLabors.ImageSharp.Processing.Transforms;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;
using Wyam.Images.Operations;

namespace Wyam.Images
{
    /// <summary>
    /// This module manipulates images by applying a variety of operations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This module manipulates images by applying operations such as resizing, darken/lighten, etc. This image module
    /// does not modify your original images in any way. It will create a copy of your images and produce images in the
    /// same image format as the original. It relies on other modules such as <c>ReadFiles</c> to read the actual images as
    /// input and <c>WriteFiles</c> to write images to disk.
    /// </para>
    /// <code>
    /// Pipelines.Add("Images",
    ///   ReadFiles("*")
    ///     .Where(x => new[] { ".jpg", ".jpeg", ".gif", ".png"}.Contains(x.Path.Extension)),
    ///   Image()
    ///     .SetJpegQuality(100).Resize(400,209).SetSuffix("-thumb"),
    ///   WriteFiles("*")
    /// );
    /// </code>
    /// <para>
    /// It will produce image with similar file name as the original image with addition of suffix indicating operations
    /// that have performed, e.g. "hello-world.jpg" can result in "hello-world-w100.jpg". The module allows you to perform more
    /// than one set of processing instructions by using the fluent property <c>And</c>.
    /// </para>
    /// <code>
    /// Pipelines.Add("Images",
    ///   ReadFiles("*")
    ///     .Where(x => new[] { ".jpg", ".jpeg", ".gif", ".png"}.Contains(x.Path.Extension)),
    ///   Image()
    ///     .SetJpegQuality(100).Resize(400, 209).SetSuffix("-thumb")
    ///     .And()
    ///     .SetJpegQuality(70).Resize(400*2, 209*2).SetSuffix("-medium"),
    ///   WriteFiles("*")
    /// );
    /// </code>
    /// <para>
    /// The above configuration produces two set of new images, one with a "-thumb" suffix and the other
    /// with a "-medium" suffix.
    /// </para>
    /// </remarks>
    /// <metadata cref="Keys.RelativeFilePath" usage="Input" />
    /// <metadata cref="Keys.WritePath" usage="Output" />
    /// <category>Content</category>
    public class Image : IModule
    {
        private readonly Stack<ImageOperations> _operations = new Stack<ImageOperations>();

        /// <summary>
        /// Process images in the content of the input document.
        /// </summary>
        public Image()
        {
            _operations.Push(new ImageOperations());
        }

        /// <summary>
        /// Outputs the image as JPEG. This will override the default
        /// behavior of outputting the image as the same format.
        /// </summary>
        /// <returns>The current module instance.</returns>
        public Image OutputAsJpeg()
        {
            _operations.Peek().OutputActions.Add(
                new OutputAction((i, s) => i.SaveAsJpeg(s), x => x.ChangeExtension(".jpg")));
            return this;
        }

        /// <summary>
        /// Outputs the image as PNG. This will override the default
        /// behavior of outputting the image as the same format.
        /// </summary>
        /// <returns>The current module instance.</returns>
        public Image OutputAsPng()
        {
            _operations.Peek().OutputActions.Add(
                new OutputAction((i, s) => i.SaveAsPng(s), x => x.ChangeExtension(".png")));
            return this;
        }

        /// <summary>
        /// Outputs the image as GIF. This will override the default
        /// behavior of outputting the image as the same format.
        /// </summary>
        /// <returns>The current module instance.</returns>
        public Image OutputAsGif()
        {
            _operations.Peek().OutputActions.Add(
                new OutputAction((i, s) => i.SaveAsGif(s), x => x.ChangeExtension(".gif")));
            return this;
        }

        /// <summary>
        /// Outputs the image as BMP. This will override the default
        /// behavior of outputting the image as the same format.
        /// </summary>
        /// <returns>The current module instance.</returns>
        public Image OutputAsBmp()
        {
            _operations.Peek().OutputActions.Add(
                new OutputAction((i, s) => i.SaveAsBmp(s), x => x.ChangeExtension(".bmp")));
            return this;
        }

        /// <summary>
        /// Allows you to specify an alternate output format for the image.
        /// For example, you might use this if you want to full specify the encoder and it's properties.
        /// This will override the default behavior of outputting the image as the same format.
        /// </summary>
        /// <param name="action">An action that should write the provided image to the provided stream.</param>
        /// <param name="pathModifier">Modifies the destination path after applying the operation (for example, to set the extension).</param>
        /// <returns>The current module instance.</returns>
        public Image OutputAs(Action<Image<Rgba32>, Stream> action, Func<FilePath, FilePath> pathModifier = null)
        {
            _operations.Peek().OutputActions.Add(new OutputAction(action, pathModifier));
            return this;
        }

        /// <summary>
        /// Allows you to specify your own ImageSharp operation.
        /// </summary>
        /// <param name="operation">The operation to perform on the image.</param>
        /// <param name="pathModifier">Modifies the destination path after applying the operation.</param>
        /// <returns>The current module instance.</returns>
        public Image Operation(
            Func<IImageProcessingContext<Rgba32>, IImageProcessingContext<Rgba32>> operation,
            Func<FilePath, FilePath> pathModifier = null)
        {
            _operations.Peek().Enqueue(new ActionOperation(operation, pathModifier));
            return this;
        }

        /// <summary>
        /// Resizes the image to a certain width and height. No resizing will be performed if
        /// both width and height are set to <c>null</c>.
        /// </summary>
        /// <param name="width">The desired width. If set to <c>null</c> or <c>0</c>, the image will maintain it's original aspect ratio.</param>
        /// <param name="height">The desired height. If set to <c>null</c> or <c>0</c>, the image will maintain it's original aspect ratio.</param>
        /// <param name="anchor">The anchor position to use (if necessary).</param>
        /// <param name="mode">The resize mode to use.</param>
        /// <returns>The current module instance.</returns>
        public Image Resize(
            int? width,
            int? height,
            AnchorPositionMode anchor = AnchorPositionMode.Center,
            ResizeMode mode = ResizeMode.BoxPad)
        {
            _operations.Peek().Enqueue(new ResizeOperation(width, height, anchor, mode));
            return this;
        }

        /// <summary>
        /// Applies black and white toning to the image.
        /// </summary>
        /// <returns>The current module instance.</returns>
        public Image BlackWhite()
        {
            _operations.Peek().Enqueue(new ActionOperation(
                image => image.BlackWhite(),
                path => path.InsertSuffix("-bw")));
            return this;
        }

        /// <summary>
        /// Brightens the image.
        /// </summary>
        /// <param name="amount">
        /// The proportion of the conversion. Must be greater than or equal to 0.
        /// A value of 0 will create an image that is completely black.
        /// A value of 1 leaves the input unchanged. Other values are linear multipliers on the effect.
        /// Values of an amount over 1 are allowed.
        /// </param>
        /// <returns>The current module instance.</returns>
        public Image Brightness(float amount)
        {
            _operations.Peek().Enqueue(new ActionOperation(
                image => image.Brightness(amount),
                path => path.InsertSuffix($"-b{amount}")));
            return this;
        }

        /// <summary>
        /// Multiplies the alpha component of the image.
        /// </summary>
        /// <param name="amount">
        /// The proportion of the conversion. Must be between 0 and 1.
        /// </param>
        /// <returns>The current module instance.</returns>
        public Image Opacity(float amount)
        {
            _operations.Peek().Enqueue(new ActionOperation(
                image => image.Opacity(amount),
                path => path.InsertSuffix($"-o{amount}")));
            return this;
        }

        /// <summary>
        /// Sets the hue of the image using <c>0</c> to <c>360</c> degree values.
        /// </summary>
        /// <param name="degrees">The degrees to set.</param>
        /// <returns>The current module instance.</returns>
        public Image Hue(float degrees)
        {
            _operations.Peek().Enqueue(new ActionOperation(
                image => image.Hue(degrees),
                path => path.InsertSuffix($"-h{degrees}")));
            return this;
        }

        /// <summary>
        /// Apply vignette processing to the image with specific color, e.g. <c>Vignette(Rgba32.AliceBlue)</c>.
        /// </summary>
        /// <param name="color">The color to use for the vignette.</param>
        /// <returns>The current module instance.</returns>
        public Image Vignette(Rgba32 color)
        {
            _operations.Peek().Enqueue(new ActionOperation(
                image => image.Vignette(color),
                path => path.InsertSuffix($"-v")));
            return this;
        }

        /// <summary>
        /// Saturates the image.
        /// </summary>
        /// <param name="amount">
        /// A value of 0 is completely un-saturated. A value of 1 leaves the input unchanged.
        /// Other values are linear multipliers on the effect. Values of amount over 1 are allowed,
        /// providing super-saturated results.</param>
        /// <returns>The current module instance.</returns>
        public Image Saturate(float amount)
        {
            _operations.Peek().Enqueue(new ActionOperation(
                image => image.Saturate(amount),
                path => path.InsertSuffix($"-s{amount}")));
            return this;
        }

        /// <summary>
        /// Adjusts the contrast of the image.
        /// </summary>
        /// <param name="amount">
        /// A value of 0 will create an image that is completely gray.
        /// A value of 1 leaves the input unchanged. Other values are linear multipliers on the effect.
        /// Values of an amount over 1 are allowed, providing results with more contrast.
        /// </param>
        /// <returns>The current module instance.</returns>
        public Image Contrast(float amount)
        {
            _operations.Peek().Enqueue(new ActionOperation(
                image => image.Contrast(amount),
                path => path.InsertSuffix($"-c{amount}")));
            return this;
        }

        /// <summary>
        /// Set the suffix of the generated image, e.g. <c>SetSuffix("-medium")</c> will transform original
        /// filename "hello-world.jpg" to "hello-world-medium.jpg".
        /// </summary>
        /// <param name="suffix">The suffix to use.</param>
        /// <returns>The current module instance.</returns>
        public Image SetSuffix(string suffix)
        {
            if (string.IsNullOrWhiteSpace(suffix))
            {
                throw new ArgumentException("Please supply the suffix");
            }

            _operations.Peek().Enqueue(new ActionOperation(null, x => x.InsertSuffix(suffix)));
            return this;
        }

        /// <summary>
        /// Set the prefix of the generated image, e.g. <c>SetPrefix("medium-")</c> will transform original
        /// filename "hello-world.jpg" to "medium-hello-world.jpg".
        /// </summary>
        /// <param name="prefix">The prefix to use.</param>
        /// <returns>The current module instance.</returns>
        public Image SetPrefix(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                throw new ArgumentException("Please supply the prefix");
            }

            _operations.Peek().Enqueue(new ActionOperation(null, x => x.InsertPrefix(prefix)));
            return this;
        }

        /// <summary>
        /// Mark the beginning of another set of processing instructions to be applied to the images.
        /// </summary>
        /// <returns>The current module instance.</returns>
        public Image And()
        {
            _operations.Push(new ImageOperations());
            return this;
        }

        /// <inheritdoc />
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            ImageFormatManager formatManager = new ImageFormatManager();
            return inputs.SelectMany(context, input =>
            {
                FilePath relativePath = input.FilePath(Keys.RelativeFilePath);
                return _operations
                    .SelectMany(operations =>
                    {
                        FilePath destinationPath = relativePath == null ? null : context.FileSystem.GetOutputPath(relativePath);

                        // Get the image
                        Image<Rgba32> image;
                        IImageFormat imageFormat;
                        using (Stream stream = input.GetStream())
                        {
                            image = SixLabors.ImageSharp.Image.Load(stream, out imageFormat);
                        }

                        // Mutate the image with the specified operations, if there are any
                        if (operations.Operations.Count > 0)
                        {
                            image.Mutate(imageContext =>
                            {
                                IImageProcessingContext<Rgba32> workingImageContext = imageContext;
                                foreach (IImageOperation operation in operations.Operations)
                                {
                                    // Apply operation
                                    workingImageContext = operation.Apply(workingImageContext);

                                    // Modify the path
                                    if (destinationPath != null)
                                    {
                                        destinationPath = operation.GetPath(destinationPath) ?? destinationPath;
                                    }
                                }
                            });
                        }

                        // Invoke output actions
                        IEnumerable<OutputAction> outputActions = operations.OutputActions.Count == 0
                            ? (IEnumerable<OutputAction>)new[] { new OutputAction((i, s) => i.Save(s, imageFormat), null) }
                            : operations.OutputActions;
                        return outputActions.Select(action =>
                        {
                            FilePath formatPath = action.GetPath(destinationPath) ?? destinationPath;
                            Trace.Verbose($"{Keys.WritePath}: {formatPath}");
                            MemoryStream outputStream = new MemoryStream();
                            action.Invoke(image, outputStream);
                            outputStream.Seek(0, SeekOrigin.Begin);
                            return context.GetDocument(input, outputStream, new MetadataItems
                            {
                                { Keys.WritePath, formatPath }
                            });
                        });
                    });
            });
        }
    }
}
