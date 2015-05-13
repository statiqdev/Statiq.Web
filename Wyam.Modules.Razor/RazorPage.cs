using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Modules.Razor
{
    // Mostly from https://github.com/aspnet/Mvc/blob/dev/src/Microsoft.AspNet.Mvc.Razor/RazorPage.cs
    public class RazorPage : IRazorPage
    {
        public void Write(object value)
        {
            throw new NotImplementedException();
        }

        public void WriteTo(TextWriter writer, object value)
        {
            throw new NotImplementedException();
        }

        public void WriteLiteral(object value)
        {
            throw new NotImplementedException();
        }

        public void WriteLiteralTo(TextWriter writer, object value)
        {
            throw new NotImplementedException();
        }

        public Task ExecuteAsync()
        {
            throw new NotImplementedException();
        }

        public string Layout { get; set; }
        public void WriteAttribute(string name, PositionTagged<string> prefix, PositionTagged<string> suffix, params AttributeValue[] values)
        {
            throw new NotImplementedException();
        }

        public void WriteAttributeTo(TextWriter writer, string name, PositionTagged<string> prefix, PositionTagged<string> suffix,
            params AttributeValue[] values)
        {
            throw new NotImplementedException();
        }

        public void BeginContext(int position, int length, bool isLiteral)
        {
            throw new NotImplementedException();
        }

        public void EndContext()
        {
            throw new NotImplementedException();
        }

        public void DefineSection(string name, RenderAsyncDelegate section)
        {
            throw new NotImplementedException();
        }

        public string Href(string contentPath)
        {
            throw new NotImplementedException();
        }
    }
}
