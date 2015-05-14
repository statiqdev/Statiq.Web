using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Modules.Razor
{
    public interface IRazorPage
    {
        // These match the default member names in Microsoft.AspNet.Razor.Generator.GeneratedClassContext
        // They must also match the string literals passed into GeneratedClassContext in the Razor module
        void Write(object value);
        void WriteTo(TextWriter writer, object value);
        void WriteLiteral(object value);
        void WriteLiteralTo(TextWriter writer, object value);
        Task ExecuteAsync();
        string Layout { get; set; }
        void WriteAttribute(string name, PositionTagged<string> prefix, PositionTagged<string> suffix, params AttributeValue[] values);
        void WriteAttributeTo(TextWriter writer, string name, PositionTagged<string> prefix, PositionTagged<string> suffix, params AttributeValue[] values);
        void BeginContext(int position, int length, bool isLiteral);
        void EndContext();
        void DefineSection(string name, RenderAsyncDelegate section);
        string Href(string contentPath);

        // Other members not required by the Razor code generator
        ViewContext ViewContext { get; set; }
        Action<TextWriter> RenderBodyDelegate { get; set; }
        bool IsLayoutBeingRendered { get; set; }
        string Path { get; set; }
        bool IsPartial { get; set; }
        IDictionary<string, RenderAsyncDelegate> PreviousSectionWriters { get; set; }
        IDictionary<string, RenderAsyncDelegate> SectionWriters { get; }
        void EnsureRenderedBodyOrSections();
    }
}
