using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Modules.Razor
{
    // From https://github.com/aspnet/Mvc/blob/dev/src/Microsoft.AspNet.Mvc.Razor/RazorPage.cs
    public abstract class RazorPage : IRazorPage
    {
        private readonly HashSet<string> _renderedSections = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private bool _renderedBody;

        protected RazorPage()
        {
            SectionWriters = new Dictionary<string, RenderAsyncDelegate>(StringComparer.OrdinalIgnoreCase);
        }

        public string Path { get; set; }

        public ViewContext ViewContext { get; set; }
        
        public string Layout { get; set; }

        public bool IsPartial { get; set; }

        public virtual TextWriter Output
        {
            get
            {
                if (ViewContext == null)
                {
                    throw new InvalidOperationException("ViewContext must be set.");
                }

                return ViewContext.Writer;
            }
        }
        
        public Action<TextWriter> RenderBodyDelegate { get; set; }

        public bool IsLayoutBeingRendered { get; set; }

        public IDictionary<string, RenderAsyncDelegate> PreviousSectionWriters { get; set; }

        public IDictionary<string, RenderAsyncDelegate> SectionWriters { get; private set; }

        public abstract Task ExecuteAsync();
        
        public virtual void Write(object value)
        {
            WriteTo(Output, value);
        }

        public virtual void WriteTo(TextWriter writer, object value)
        {
            WriteTo(writer, value, escapeQuotes: false);
        }

        public static void WriteTo(
            TextWriter writer,
            object value,
            bool escapeQuotes)
        {
            if (value == null || value == HtmlString.Empty)
            {
                return;
            }

            var helperResult = value as HelperResult;
            if (helperResult != null)
            {
                helperResult.WriteTo(writer);
                return;
            }

            var htmlString = value as HtmlString;
            if (htmlString != null)
            {
                if (escapeQuotes)
                {
                    // In this case the text likely came directly from the Razor source. Since the original string is
                    // an attribute value that may have been quoted with single quotes, must handle any double quotes
                    // in the value. Writing the value out surrounded by double quotes.
                    //
                    // Do not combine following condition with check of escapeQuotes; htmlString.ToString() can be
                    // expensive when the HtmlString is created with a StringCollectionTextWriter.
                    var stringValue = htmlString.ToString();
                    if (stringValue.Contains("\""))
                    {
                        writer.Write(stringValue.Replace("\"", "&quot;"));
                        return;
                    }
                }

                htmlString.WriteTo(writer);
                return;
            }

            WriteTo(writer, value.ToString());
        }

        private static void WriteTo(TextWriter writer, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                WebUtility.HtmlEncode(value, writer);
            }
        }

        public virtual void WriteLiteral(object value)
        {
            WriteLiteralTo(Output, value);
        }

        public virtual void WriteLiteralTo(TextWriter writer, object value)
        {
            if (value != null)
            {
                WriteLiteralTo(writer, value.ToString());
            }
        }

        public virtual void WriteLiteralTo(TextWriter writer, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                writer.Write(value);
            }
        }

        public virtual void WriteAttribute(
            string name,
            PositionTagged<string> prefix,
            PositionTagged<string> suffix,
            params AttributeValue[] values)
        {
            WriteAttributeTo(Output, name, prefix, suffix, values);
        }

        public virtual void WriteAttributeTo(
            TextWriter writer,
            string name,
            PositionTagged<string> prefix,
            PositionTagged<string> suffix,
            params AttributeValue[] values)
        {
            var first = true;
            var wroteSomething = false;
            if (values.Length == 0)
            {
                // Explicitly empty attribute, so write the prefix and suffix
                WritePositionTaggedLiteral(writer, prefix);
                WritePositionTaggedLiteral(writer, suffix);
            }
            else
            {
                for (var i = 0; i < values.Length; i++)
                {
                    var attrVal = values[i];
                    var val = attrVal.Value;
                    var next = i == values.Length - 1 ?
                        suffix : // End of the list, grab the suffix
                        values[i + 1].Prefix; // Still in the list, grab the next prefix

                    if (val.Value == null)
                    {
                        // Nothing to write
                        continue;
                    }

                    // The special cases here are that the value we're writing might already be a string, or that the
                    // value might be a bool. If the value is the bool 'true' we want to write the attribute name
                    // instead of the string 'true'. If the value is the bool 'false' we don't want to write anything.
                    // Otherwise the value is another object (perhaps an HtmlString) and we'll ask it to format itself.
                    string stringValue;

                    // Intentionally using is+cast here for performance reasons. This is more performant than as+bool?
                    // because of boxing.
                    if (val.Value is bool)
                    {
                        if ((bool)val.Value)
                        {
                            stringValue = name;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        stringValue = val.Value as string;
                    }

                    if (first)
                    {
                        WritePositionTaggedLiteral(writer, prefix);
                        first = false;
                    }
                    else
                    {
                        WritePositionTaggedLiteral(writer, attrVal.Prefix);
                    }

                    // Calculate length of the source span by the position of the next value (or suffix)
                    var sourceLength = next.Position - attrVal.Value.Position;

                    BeginContext(attrVal.Value.Position, sourceLength, isLiteral: attrVal.Literal);
                    // The extra branching here is to ensure that we call the Write*To(string) overload where
                    // possible.
                    if (attrVal.Literal && stringValue != null)
                    {
                        WriteLiteralTo(writer, stringValue);
                    }
                    else if (attrVal.Literal)
                    {
                        WriteLiteralTo(writer, val.Value);
                    }
                    else if (stringValue != null)
                    {
                        WriteTo(writer, stringValue);
                    }
                    else
                    {
                        WriteTo(writer, val.Value);
                    }

                    EndContext();
                    wroteSomething = true;
                }
                if (wroteSomething)
                {
                    WritePositionTaggedLiteral(writer, suffix);
                }
            }
        }

        public virtual string Href(string contentPath)
        {
            return contentPath;
        }

        private void WritePositionTaggedLiteral(TextWriter writer, string value, int position)
        {
            BeginContext(position, value.Length, isLiteral: true);
            WriteLiteralTo(writer, value);
            EndContext();
        }

        private void WritePositionTaggedLiteral(TextWriter writer, PositionTagged<string> value)
        {
            WritePositionTaggedLiteral(writer, value.Value, value.Position);
        }

        protected virtual HelperResult RenderBody()
        {
            if (RenderBodyDelegate == null)
            {
                throw new InvalidOperationException("RenderBody cannot be called.");
            }

            _renderedBody = true;
            return new HelperResult(RenderBodyDelegate);
        }

        public void DefineSection(string name, RenderAsyncDelegate section)
        {
            if (SectionWriters.ContainsKey(name))
            {
                throw new InvalidOperationException("Section " + name + " already defined.");
            }
            SectionWriters[name] = section;
        }

        public bool IsSectionDefined(string name)
        {
            EnsureMethodCanBeInvoked("IsSectionDefined");
            return PreviousSectionWriters.ContainsKey(name);
        }

        public HtmlString RenderSection(string name)
        {
            return RenderSection(name, required: true);
        }

        public HtmlString RenderSection(string name, bool required)
        {
            EnsureMethodCanBeInvoked("RenderSection");

            var task = RenderSectionAsyncCore(name, required);
            return TaskHelper.WaitAndThrowIfFaulted(task);
        }

        public Task<HtmlString> RenderSectionAsync(string name)
        {
            return RenderSectionAsync(name, required: true);
        }

        public async Task<HtmlString> RenderSectionAsync(string name, bool required)
        {
            EnsureMethodCanBeInvoked("RenderSectionAsync");
            return await RenderSectionAsyncCore(name, required);
        }

        private async Task<HtmlString> RenderSectionAsyncCore(string sectionName, bool required)
        {
            if (_renderedSections.Contains(sectionName))
            {
                throw new InvalidOperationException("Section " + sectionName + " already rendered.");
            }

            RenderAsyncDelegate renderDelegate;
            if (PreviousSectionWriters.TryGetValue(sectionName, out renderDelegate))
            {
                _renderedSections.Add(sectionName);
                await renderDelegate(Output);

                // Return a token value that allows the Write call that wraps the RenderSection \ RenderSectionAsync
                // to succeed.
                return HtmlString.Empty;
            }
            else if (required)
            {
                // If the section is not found, and it is not optional, throw an error.
                throw new InvalidOperationException("Section " + sectionName + " not defined.");
            }
            else
            {
                // If the section is optional and not found, then don't do anything.
                return null;
            }
        }

        public void EnsureRenderedBodyOrSections()
        {
            // a) all sections defined for this page are rendered.
            // b) if no sections are defined, then the body is rendered if it's available.
            if (PreviousSectionWriters != null && PreviousSectionWriters.Count > 0)
            {
                var sectionsNotRendered = PreviousSectionWriters.Keys.Except(
                    _renderedSections,
                    StringComparer.OrdinalIgnoreCase);

                if (sectionsNotRendered.Any())
                {
                    var sectionNames = string.Join(", ", sectionsNotRendered);
                    throw new InvalidOperationException("Sections " + sectionNames + " not rendered.");
                }
            }
            else if (RenderBodyDelegate != null && !_renderedBody)
            {
                throw new InvalidOperationException("RenderBody not called.");
            }
        }

        public void BeginContext(int position, int length, bool isLiteral)
        {
        }

        public void EndContext()
        {
        }

        private void EnsureMethodCanBeInvoked(string methodName)
        {
            if (PreviousSectionWriters == null)
            {
                throw new InvalidOperationException("Method " + methodName + " cannot be called.");
            }
        }
    }
}
