using ImageProcessor.Imaging.Filters.Photo;
using System.Collections.Generic;
using System.Drawing;

namespace Wyam.Modules.ImageProcessor
{
    public class ImageInstruction
    {
        public int? Width { get; set; }

        public int? Height { get; set; }

        public int? Brightness { get; set; }

        public List<ImageFilter> Filters { get; set; } = new List<ImageFilter>();

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
}
