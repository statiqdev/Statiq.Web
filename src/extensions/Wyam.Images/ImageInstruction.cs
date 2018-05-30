using System;
using System.Collections.Generic;
using System.Drawing;
using SixLabors.ImageSharp.Processing.Transforms;

namespace Wyam.Images
{

    public class HueInstruction
    {
        public int Degrees { get; set; }
    }

    public class ImageInstruction
    {
        public int? Width { get; set; }

        public int? Height { get; set; }

        public Size? Constraint { get; set; }

        public AnchorPositionMode AnchorPosition { get; set; }

        public int? Brightness { get; set; }

        public int? Opacity { get; set; }

        public HueInstruction Hue { get; set; }

        public Color? Tint { get; set; }

        public Color? Vignette { get; set; }

        public int JpegQuality { get; set; } = 70;

        public int? Saturation { get; set; }

        public int? Contrast { get; set; }

        public string FileNamePrefix { get; set; }

        public string FileNameSuffix { get; set; }

        public bool IsFileNameCustomized => !string.IsNullOrWhiteSpace(FileNamePrefix) || !string.IsNullOrWhiteSpace(FileNameSuffix);

        public List<ImageFilter> Filters { get; set; } = new List<ImageFilter>();

        public SixLabors.Primitives.Size? GetCropSize()
        {
            if (Width.HasValue && Height.HasValue)
            {
                return new SixLabors.Primitives.Size(Width.Value, Height.Value);
            }
            else if (Width.HasValue)
            {
                return new SixLabors.Primitives.Size(Width.Value, 0);
            }
            else if (Height.HasValue)
            {
                return new SixLabors.Primitives.Size(0, Height.Value);
            }

            return null;
        }

        public bool IsNeedResize => GetCropSize() != null;

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
            {
                suffix += "-w" + Width.Value;
            }

            if (Height.HasValue)
            {
                suffix += "-h" + Height.Value;
            }

            if (Constraint.HasValue)
            {
                suffix += $"-cw{Constraint.Value.Width}h{Constraint.Value.Height}";
            }

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

            if (Opacity.HasValue)
            {
                suffix += $"-o{Opacity.Value}";
            }

            if (Hue != null)
            {
                suffix += $"-h{Hue.Degrees}";
            }

            if (Tint.HasValue)
            {
                suffix += $"-t{Tint.Value.ToString().Replace("Color [", "").Replace("]", "")}";
            }

            if (Vignette.HasValue)
            {
                suffix += $"-v{Vignette.Value.ToString().Replace("Color [", "").Replace("]", "")}";
            }

            if (Saturation.HasValue && Saturation > 0)
            {
                suffix += $"-s{Saturation.Value}";
            }

            if (Saturation.HasValue && Saturation < 0)
            {
                suffix += $"-ds{Saturation.Value * -1}"; //only shows positive number
            }

            if (Contrast.HasValue)
            {
                suffix += $"-c{Contrast.Value}";
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

        public AnchorPositionMode GetAnchorPosition()
        {
            if (!Enum.IsDefined(typeof(AnchorPositionMode), AnchorPosition))
            {
                return AnchorPositionMode.Center;
            }

            return AnchorPosition;
        }
    }
}
