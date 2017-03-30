using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;
using Wyam.Common.Util;
using img = ImageProcessor;

namespace Wyam.Images
{
    /// <summary>
    /// This module manipulates images by applying a variety of operations.
    /// </summary>
    /// <remarks>
    /// <para>This module manipulates images by applying operations such as resizing, darken/lighten, etc. This image module
    /// does not modify your original images in anyway.It will create a copy of your images and produce images in the
    /// same image format as the original. It relies on other modules such as <c>ReadFiles</c> to read the actual images as
    /// input and <c>WriteFiles</c> to write images to disk.</para>
    /// <code>
    /// Pipelines.Add("Images",
    ///   ReadFiles("*")
    ///     .Where(x => new[] { ".jpg", ".jpeg", ".gif", ".png"}.Contains(x.Path.Extension)),
    ///   Image()
    ///     .SetJpegQuality(100).Resize(400,209).SetSuffix("-thumb"),
    ///   WriteFiles("*")
    /// );
    /// </code>
    /// <para>It will produce image with similar file name as the original image with addition of suffix indicating operations
    /// that have performed, e.g. "hello-world.jpg" can result in "hello-world-w100.jpg". The module allows you to perform more
    /// than one set of processing instructions by using the fluent property <c>And</c>.</para>
    /// <code>
    /// Pipelines.Add("Images",
    ///   ReadFiles("*")
    ///     .Where(x => new[] { ".jpg", ".jpeg", ".gif", ".png"}.Contains(x.Path.Extension)),
    ///   Image()
    ///     .SetJpegQuality(100).Resize(400,209).SetSuffix("-thumb")
    ///     .And
    ///     .SetJpegQuality(70).Resize(400*2, 209*2).SetSuffix("-medium"),
    ///   WriteFiles("*")
    /// );
    /// </code>
    /// <para>The above configuration produces two set of new images, one with a "-thumb" suffix and the other with a "-medium" suffix.</para>
    /// </remarks>
    /// <category>Content</category>
    public class Image : IModule
    {
        private readonly List<ImageInstruction> _instructions;

        private ImageInstruction _currentInstruction;

        /// <summary>
        /// Creates the module without any instructions.
        /// </summary>
        public Image()
        {
            _instructions = new List<ImageInstruction>();
        }

        private void EnsureCurrentInstruction()
        {
            if (_currentInstruction == null)
            {
                _currentInstruction = new ImageInstruction();
                _instructions.Add(_currentInstruction);
            }
        }

        /// <summary>
        /// Resizes the image to a certain width and height. It will crop the image whenever necessary. The module will not perform
        /// any image resizing if both width and height are set to <c>null</c>. If the source image is smaller than the specified
        /// width and height, the image will be enlarged.
        /// </summary>
        /// <param name="width">The desired width. If set to <c>null</c> or <c>0</c>, the image will be resized to its height.</param>
        /// <param name="height">The desired height. If set to <c>null</c> or <c>0</c>, the image will be resized to its width.</param>
        /// <param name="anchor">The anchor position to use for cropping (if necessary). The available values are:
        /// <list type="bullet">
        /// <item><description>AnchorPosition.Center</description></item>
        /// <item><description>AnchorPosition.Top</description></item>
        /// <item><description>AnchorPosition.Bottom</description></item>
        /// <item><description>AnchorPosition.Left</description></item>
        /// <item><description>AnchorPosition.Right</description></item>
        /// <item><description>AnchorPosition.TopLeft</description></item>
        /// <item><description>AnchorPosition.TopRight</description></item>
        /// <item><description>AnchorPosition.BottomLeft</description></item>
        /// <item><description>AnchorPosition.BottomRight</description></item>
        /// </list>
        /// </param>
        public Image Resize(int? width, int? height, AnchorPosition anchor = AnchorPosition.Center)
        {
            EnsureCurrentInstruction();

            _currentInstruction.Width = width;
            _currentInstruction.Height = height;
            _currentInstruction.AnchorPosition = anchor;

            return this;
        }

        /// <summary>
        /// Constrains the image to a specified size. If the image is larger than the specified <c>width</c> and <c>height</c>, it will
        /// be resized down. If the image is smaller than the specified <c>width</c> and <c>height</c>, it will not be resized.
        /// </summary>
        /// <param name="width">The maximum desired width.</param>
        /// <param name="height">The maximum desired height.</param>
        public Image Constrain(int width, int height)
        {
            EnsureCurrentInstruction();

            _currentInstruction.Constraint = new Size(width, height);

            return this;
        }

        /// <summary>
        /// Applies the the specified image filters. The available filters are:
        /// <list type="bullet">
        /// <item><description>ImageFilter.BlackAndWhite</description></item>
        /// <item><description>ImageFilter.Comic</description></item>
        /// <item><description>ImageFilter.Gotham</description></item>
        /// <item><description>ImageFilter.GreyScale</description></item>
        /// <item><description>ImageFilter.HiSatch</description></item>
        /// <item><description>ImageFilter.Invert</description></item>
        /// <item><description>ImageFilter.Lomograph</description></item>
        /// <item><description>ImageFilter.LoSatch</description></item>
        /// <item><description>ImageFilter.Polaroid</description></item>
        /// <item><description>ImageFilter.Sepia</description></item>
        /// </list>
        /// These filter values map directly to filters provided by ImageProcessor library.
        /// <a href="http://imageprocessor.org/imageprocessor/imagefactory/filter/">You can see the effects of
        /// these filters here</a>.
        /// </summary>
        /// <param name="filters">The filters to apply.</param>
        public Image ApplyFilters(params ImageFilter[] filters)
        {
            EnsureCurrentInstruction();

            _currentInstruction.Filters.AddRange(filters);

            return this;
        }

        /// <summary>
        /// Brightens the image by the specified percentage.
        /// </summary>
        /// <param name="percentage">The percentage to brighten the image by (<c>0</c> to <c>100</c>).</param>
        public Image Brighten(short percentage)
        {
            if (percentage < 0 || percentage > 100)
                throw new ArgumentException($"Percentage must be between 0 and 100 instead of {percentage}%");

            EnsureCurrentInstruction();

            _currentInstruction.Brightness = percentage;

            return this;
        }

        /// <summary>
        /// Darkens the image by the specified percentage.
        /// </summary>
        /// <param name="percentage">The percentage to darken the image by (<c>0</c> to <c>100</c>).</param>
        public Image Darken(short percentage)
        {
            if (percentage < 0 || percentage > 100)
                throw new ArgumentException($"Percentage must be between 0 and 100 instead of {percentage}%");

            EnsureCurrentInstruction();

            _currentInstruction.Brightness = -percentage;

            return this;
        }

        /// <summary>
        /// Sets the opacity of the image.
        /// </summary>
        /// <param name="percentage">The opacity percentage (<c>0</c> to <c>100</c>).</param>
        public Image SetOpacity(short percentage)
        {
            if (percentage < 0 || percentage > 100)
                throw new ArgumentException($"Percentage must be between 0 and 100 instead of {percentage}%");

            EnsureCurrentInstruction();

            _currentInstruction.Opacity = percentage;

            return this;
        }

        /// <summary>
        /// Sets the hue of the image using <c>0</c> to <c>360</c> degree values.
        /// </summary>
        /// <param name="degrees">The degrees to set.</param>
        /// <param name="rotate">If set to <c>true</c>, rotates the hue.</param>
        public Image SetHue(short degrees, bool rotate = false)
        {
            if (degrees < 0 || degrees > 360)
                throw new ArgumentException($"Degrees must be between 0 and 360 instead of {degrees}");

            EnsureCurrentInstruction();

            _currentInstruction.Hue = new HueInstruction
            {
                Degrees = degrees,
                Rotate = rotate
            };

            return this;
        }

        /// <summary>
        /// Tints the image to the specified color, e.g. <c>Color.Aqua</c>.
        /// <a href="https://msdn.microsoft.com/en-us/library/system.drawing.color(v=vs.110).aspx">Please
        /// check here for more color values</a>.
        /// </summary>
        /// <param name="color">The color to tint the image to.</param>
        public Image Tint(Color color)
        {
            EnsureCurrentInstruction();
            _currentInstruction.Tint = color;

            return this;
        }

        /// <summary>
        /// Apply vignette processing to the image with specific color, e.g. <c>Vignette(Color.AliceBlue)</c>.
        /// </summary>
        /// <param name="color">The color to use for the vignette.</param>
        public Image Vignette(Color color)
        {
            EnsureCurrentInstruction();
            _currentInstruction.Vignette = color;

            return this;
        }

        /// <summary>
        /// Saturates the image.
        /// </summary>
        /// <param name="percentage">The saturation percentage (<c>0</c> to <c>100</c>).</param>
        public Image Saturate(short percentage)
        {
            if (percentage < 0 || percentage > 100)
                throw new ArgumentException($"Percentage must be between 0 and 100 instead of {percentage}%");

            EnsureCurrentInstruction();
            _currentInstruction.Saturation = percentage;
            return this;
        }

        /// <summary>
        /// Desaturates the image.
        /// </summary>
        /// <param name="percentage">The desaturation percentage (<c>0</c> to <c>100</c>).</param>
        public Image Desaturate(short percentage)
        {
            if (percentage < 0 || percentage > 100)
                throw new ArgumentException($"Percentage must be between 0 and 100 instead of {percentage}%");

            EnsureCurrentInstruction();
            _currentInstruction.Saturation = -percentage;
            return this;
        }

        /// <summary>
        /// This setting only applies to JPEG images. It sets the quality of the JPEG output. The possible values are from <c>0</c> to <c>100</c>.
        /// </summary>
        /// <param name="quality">The desired JPEG quality (<c>0</c> to <c>100</c>).</param>
        public Image SetJpegQuality(short quality)
        {
            if (quality < 0 || quality > 100)
                throw new ArgumentException($"Quality must be between 0 and 100 instead of {quality}");

            EnsureCurrentInstruction();
            _currentInstruction.JpegQuality = quality;
            return this;
        }

        /// <summary>
        /// Adjusts the contrast of the image.
        /// </summary>
        /// <param name="percentage">Set the contrast value of the image from the value of <c>-100</c> to <c>100</c>.</param>
        public Image SetContrast(short percentage)
        {
            if (percentage < -100 || percentage > 100)
                throw new ArgumentException($"Percentage must be between -100 and 100 instead of {percentage}%");

            EnsureCurrentInstruction();
            _currentInstruction.Contrast = percentage;
            return this;
        }

        /// <summary>
        /// Set the suffix of the generated image, e.g. <c>SetSuffix("-medium")</c> will transform original
        /// filename "hello-world.jpg" to "hello-world-medium.jpg".
        /// </summary>
        /// <param name="suffix">The suffix to use.</param>
        public Image SetSuffix(string suffix)
        {
            if (string.IsNullOrWhiteSpace(suffix))
                throw new ArgumentException("Please supply the suffix");

            EnsureCurrentInstruction();
            _currentInstruction.FileNameSuffix = suffix;
            return this;
        }

        /// <summary>
        /// Set the prefix of the generated image, e.g. <c>SetPrefix("medium-")</c> will transform original
        /// filename "hello-world.jpg" to "medium-hello-world.jpg".
        /// </summary>
        /// <param name="prefix">The prefix to use.</param>
        public Image SetPrefix(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                throw new ArgumentException("Please supply the prefix");

            EnsureCurrentInstruction();
            _currentInstruction.FileNamePrefix = prefix;
            return this;
        }

        /// <summary>
        /// Mark the beginning of another set of processing instructions to be applied to the images.
        /// </summary>
        public Image And
        {
            get
            {
                _currentInstruction = null;
                return this;
            }
        }

        private ISupportedImageFormat GetFormat(string extension, ImageInstruction ins)
        {
            ISupportedImageFormat format = null;

            if (extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
                format = new JpegFormat { Quality = ins.JpegQuality };
            else
                if (extension.Equals(".gif", StringComparison.OrdinalIgnoreCase))
                format = new GifFormat { };
            else if (extension.Equals(".png", StringComparison.OrdinalIgnoreCase))
                format = new PngFormat { };

            return format;
        }

        /// <inheritdoc />
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.SelectMany(context, input =>
            {
                FilePath relativePath = input.FilePath(Keys.RelativeFilePath);
                if (relativePath == null)
                {
                    return Array.Empty<IDocument>();
                }
                FilePath destinationPath = context.FileSystem.GetOutputPath(relativePath);

                return _instructions.Select(instruction =>
                {
                    ISupportedImageFormat format = GetFormat(relativePath.Extension, instruction);
                    if (format == null)
                    {
                        return null;
                    }

                    string destinationFile = relativePath.FileNameWithoutExtension.FullPath;

                    if (instruction.IsFileNameCustomized)
                    {
                        if (!string.IsNullOrWhiteSpace(instruction.FileNamePrefix))
                        {
                            destinationFile = instruction.FileNamePrefix + destinationFile;
                        }

                        if (!string.IsNullOrWhiteSpace(instruction.FileNameSuffix))
                        {
                            destinationFile += instruction.FileNameSuffix + relativePath.Extension;
                        }
                    }
                    else
                    {
                        destinationFile += instruction.GetSuffix() + relativePath.Extension;
                    }

                    destinationPath = destinationPath.Directory.CombineFile(destinationFile);
                    Trace.Verbose($"{Keys.WritePath}: {destinationPath}");

                    Stream output = ProcessImage(input, format, instruction);

                    return context.GetDocument(input, output, new MetadataItems
                    {
                        {Keys.WritePath, destinationPath},
                        {Keys.WriteExtension, relativePath.Extension}
                    });
                }).Where(x => x != null);
            });
        }

        private Stream ProcessImage(IDocument input, ISupportedImageFormat format, ImageInstruction ins)
        {
            using (img.ImageFactory imageFactory = new img.ImageFactory(preserveExifData: true))
            {
                // Load, resize, set the format and quality and save an image.
                img.ImageFactory fac;
                using (Stream stream = input.GetStream())
                {
                    fac = imageFactory.Load(stream).Format(format);
                }

                if (ins.IsNeedResize)
                {
                    if (ins.IsCropRequired)
                    {
                        var layer = new ResizeLayer(
                            size: ins.GetCropSize().Value,
                            anchorPosition: ins.GetAnchorPosition(),
                            resizeMode: ResizeMode.Crop
                            );

                        fac.Resize(layer);
                    }
                    else
                    {
                        fac.Resize(ins.GetCropSize().Value);
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

                if (ins.Opacity.HasValue)
                {
                    fac.Alpha(ins.Opacity.Value);
                }

                if (ins.Hue != null)
                {
                    fac.Hue(ins.Hue.Degrees, ins.Hue.Rotate);
                }

                if (ins.Tint != null)
                {
                    fac.Tint(ins.Tint.Value);
                }

                if (ins.Vignette != null)
                {
                    fac.Vignette(ins.Vignette.Value);
                }

                if (ins.Saturation.HasValue)
                {
                    fac.Saturation(ins.Saturation.Value);
                }

                if (ins.Contrast.HasValue)
                {
                    fac.Contrast(ins.Contrast.Value);
                }

                var outputStream = new MemoryStream();
                fac.Save(outputStream);
                outputStream.Seek(0, SeekOrigin.Begin);
                return outputStream;
            }
        }
    }
}
