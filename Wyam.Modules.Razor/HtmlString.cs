using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Modules.Razor
{
    // From https://github.com/aspnet/Mvc/blob/dev/src/Microsoft.AspNet.Mvc.Core/Rendering/HtmlString.cs
    public class HtmlString
    {
        private static readonly HtmlString _empty = new HtmlString(string.Empty);

        private readonly StringCollectionTextWriter _writer;
        private readonly string _input;

        public HtmlString(string input)
        {
            _input = input;
        }

        public HtmlString(StringCollectionTextWriter writer)
        {
            _writer = writer;
        }

        public static HtmlString Empty
        {
            get
            {
                return _empty;
            }
        }

        public void WriteTo(TextWriter targetWriter)
        {
            if (_writer != null)
            {
                _writer.CopyTo(targetWriter);
            }
            else
            {
                targetWriter.Write(_input);
            }
        }

        public override string ToString()
        {
            if (_writer != null)
            {
                return _writer.ToString();
            }

            return _input;
        }
    }
}
