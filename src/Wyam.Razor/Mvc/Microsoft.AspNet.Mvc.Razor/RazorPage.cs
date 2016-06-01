// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Wyam.Razor.Microsoft.AspNet.Html.Abstractions;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Execution;
using Wyam.Common.Tracing;
using Wyam.Razor.Microsoft.Framework.Internal;
using HtmlString = Wyam.Razor.Microsoft.AspNet.Mvc.Rendering.HtmlString;

namespace Wyam.Razor.Microsoft.AspNet.Mvc.Razor
{
    public abstract class RazorPage : IRazorPage
    {
        private readonly HashSet<string> _renderedSections = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private bool _renderedBody;

        public RazorPage()
        {
            SectionWriters = new Dictionary<string, RenderAsyncDelegate>(StringComparer.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public string Path { get; set; }

        /// <inheritdoc />
        public ViewContext ViewContext { get; set; }

        /// <inheritdoc />
        public string Layout { get; set; }

        /// <inheritdoc />
        public bool IsPartial { get; set; }

        public IMetadata Metadata
        {
            get { return ViewContext.Document; }
        }

        public IDocument Document
        {
            get { return ViewContext.Document; }
        }

        // Expose a Model property to better match existing conventions
        public IDocument Model
        {
            get { return ViewContext.Document; }
        }

        // In ASP.NET MVC these kinds of properties are injected with InjectChunk in MvcRazorHost, but this is easier
        public HtmlHelper Html
        {
            get { return new HtmlHelper(ViewContext); }
        }

        public IExecutionContext Context
        {
            get { return ViewContext.ExecutionContext; }
        }

        public IExecutionContext ExecutionContext
        {
            get { return ViewContext.ExecutionContext; }
        }

        public IDocumentCollection Documents
        {
            get { return ExecutionContext.Documents; }
        }

        // Define Trace as a property so it's not ambiguous with System.Diagnostics.Trace
        public ITrace Trace
        {
            get { return Wyam.Common.Tracing.Trace.Current; }
        }

        /// <summary>
        /// Gets the TextWriter that the page is writing output to.
        /// </summary>
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

        public IDictionary<string, object> ViewData
        {
            get { return ViewContext == null ? null : ViewContext.ViewData; }
        }
        
        public dynamic ViewBag
        {
            get
            {
                return ViewContext == null ? null : ViewContext.ViewBag;
            }
        }

        /// <inheritdoc />
        public Action<TextWriter> RenderBodyDelegate { get; set; }

        /// <inheritdoc />
        public bool IsLayoutBeingRendered { get; set; }

        /// <inheritdoc />
        public IDictionary<string, RenderAsyncDelegate> PreviousSectionWriters { get; set; }

        /// <inheritdoc />
        public IDictionary<string, RenderAsyncDelegate> SectionWriters { get; private set; }

        /// <inheritdoc />
        public abstract Task ExecuteAsync();

        /// <summary>
        /// Writes the specified <paramref name="value"/> with HTML encoding to <see cref="Output"/>.
        /// </summary>
        /// <param name="value">The <see cref="object"/> to write.</param>
        public virtual void Write(object value)
        {
            WriteTo(Output, value);
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> with HTML encoding to <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="value">The <see cref="object"/> to write.</param>
        /// <remarks>
        /// <paramref name="value"/>s of type <see cref="IHtmlContent"/> are written using 
        /// <see cref="IHtmlContent.WriteTo(TextWriter, IHtmlEncoder)"/>.
        /// For all other types, the encoded result of <see cref="object.ToString"/> is written to the
        /// <paramref name="writer"/>.
        /// </remarks>
        public virtual void WriteTo([NotNull] TextWriter writer, object value)
        {
            WriteTo(writer, value, escapeQuotes: false);
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> with HTML encoding to given <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="encoder">The <see cref="IHtmlEncoder"/> to use when encoding <paramref name="value"/>.</param>
        /// <param name="value">The <see cref="object"/> to write.</param>
        /// <param name="escapeQuotes">
        /// If <c>true</c> escapes double quotes in a <paramref name="value"/> of type <see cref="HtmlString"/>.
        /// Otherwise writes <see cref="HtmlString"/> values as-is.
        /// </param>
        /// <remarks>
        /// <paramref name="value"/>s of type <see cref="IHtmlContent"/> are written using 
        /// <see cref="IHtmlContent.WriteTo(TextWriter, IHtmlEncoder)"/>.
        /// For all other types, the encoded result of <see cref="object.ToString"/> is written to the
        /// <paramref name="writer"/>.
        /// </remarks>
        public static void WriteTo(
            [NotNull] TextWriter writer,
            object value,
            bool escapeQuotes)
        {
            if (value == null || value == HtmlString.Empty)
            {
                return;
            }

            // Wyam - check for HelperResult
            var helperResult = value as HelperResult;
            if (helperResult != null)
            {
                helperResult.WriteTo(writer);
                return;
            }

            var htmlContent = value as IHtmlContent;
            if (htmlContent != null)
            {
                if (escapeQuotes)
                {
                    // In this case the text likely came directly from the Razor source. Since the original string is
                    // an attribute value that may have been quoted with single quotes, must handle any double quotes
                    // in the value. Writing the value out surrounded by double quotes.
                    //
                    // Do not combine following condition with check of escapeQuotes; htmlContent.ToString() can be
                    // expensive when the IHtmlContent is created with a BufferedHtmlContent.
                    var stringValue = htmlContent.ToString();
                    if (stringValue.Contains("\""))
                    {
                        writer.Write(stringValue.Replace("\"", "&quot;"));
                        return;
                    }
                }

                htmlContent.WriteTo(writer);
                return;
            }

            // Wyam - One more check to see if it's an old-school IHtmlString
            var iHtmlString = value as IHtmlString;
            if (iHtmlString != null)
            {
                writer.Write(iHtmlString.ToHtmlString());
            }

            WriteTo(writer, value.ToString());
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> with HTML encoding to <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="value">The <see cref="string"/> to write.</param>
        public static void WriteTo([NotNull] TextWriter writer, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                WebUtility.HtmlEncode(value, writer);
            }
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> without HTML encoding to <see cref="Output"/>.
        /// </summary>
        /// <param name="value">The <see cref="object"/> to write.</param>
        public virtual void WriteLiteral(object value)
        {
            WriteLiteralTo(Output, value);
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> without HTML encoding to the <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="value">The <see cref="object"/> to write.</param>
        public virtual void WriteLiteralTo([NotNull] TextWriter writer, object value)
        {
            if (value != null)
            {
                WriteLiteralTo(writer, value.ToString());
            }
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> without HTML encoding to <see cref="Output"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="value">The <see cref="string"/> to write.</param>
        public virtual void WriteLiteralTo([NotNull] TextWriter writer, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                writer.Write(value);
            }
        }

        public virtual void WriteAttribute(
            string name,
            [NotNull] PositionTagged<string> prefix,
            [NotNull] PositionTagged<string> suffix,
            params AttributeValue[] values)
        {
            WriteAttributeTo(Output, name, prefix, suffix, values);
        }

        public virtual void WriteAttributeTo(
            [NotNull] TextWriter writer,
            string name,
            [NotNull] PositionTagged<string> prefix,
            [NotNull] PositionTagged<string> suffix,
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

                    if (!string.IsNullOrEmpty(attrVal.Prefix))
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

        public virtual string Href([NotNull] string contentPath)
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

        /// <summary>
        /// Creates a named content section in the page that can be invoked in a Layout page using
        /// <see cref="RenderSection(string)"/> or <see cref="RenderSectionAsync(string, bool)"/>.
        /// </summary>
        /// <param name="name">The name of the section to create.</param>
        /// <param name="section">The <see cref="RenderAsyncDelegate"/> to execute when rendering the section.</param>
        public void DefineSection([NotNull] string name, [NotNull] RenderAsyncDelegate section)
        {
            if (SectionWriters.ContainsKey(name))
            {
                throw new InvalidOperationException("Section " + name + " already defined.");
            }
            SectionWriters[name] = section;
        }

        public bool IsSectionDefined([NotNull] string name)
        {
            EnsureMethodCanBeInvoked("IsSectionDefined");
            return PreviousSectionWriters.ContainsKey(name);
        }

        /// <summary>
        /// In layout pages, renders the content of the section named <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the section to render.</param>
        /// <returns>Returns <see cref="HtmlString.Empty"/> to allow the <see cref="Write(object)"/> call to
        /// succeed.</returns>
        /// <remarks>The method writes to the <see cref="Output"/> and the value returned is a token
        /// value that allows the Write (produced due to @RenderSection(..)) to succeed. However the
        /// value does not represent the rendered content.</remarks>
        public HtmlString RenderSection([NotNull] string name)
        {
            return RenderSection(name, required: true);
        }

        /// <summary>
        /// In layout pages, renders the content of the section named <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The section to render.</param>
        /// <param name="required">Indicates if this section must be rendered.</param>
        /// <returns>Returns <see cref="HtmlString.Empty"/> to allow the <see cref="Write(object)"/> call to
        /// succeed.</returns>
        /// <remarks>The method writes to the <see cref="Output"/> and the value returned is a token
        /// value that allows the Write (produced due to @RenderSection(..)) to succeed. However the
        /// value does not represent the rendered content.</remarks>
        public HtmlString RenderSection([NotNull] string name, bool required)
        {
            EnsureMethodCanBeInvoked("RenderSection");

            var task = RenderSectionAsyncCore(name, required);
            return task.GetAwaiter().GetResult();
        }

        /// <summary>
        /// In layout pages, asynchronously renders the content of the section named <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The section to render.</param>
        /// <returns>A <see cref="Task{HtmlString}"/> that on completion returns <see cref="HtmlString.Empty"/> that
        /// allows the <see cref="Write(object)"/> call to succeed.</returns>
        /// <remarks>The method writes to the <see cref="Output"/> and the value returned is a token
        /// value that allows the Write (produced due to @RenderSection(..)) to succeed. However the
        /// value does not represent the rendered content.</remarks>
        public Task<HtmlString> RenderSectionAsync([NotNull] string name)
        {
            return RenderSectionAsync(name, required: true);
        }

        /// <summary>
        /// In layout pages, asynchronously renders the content of the section named <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The section to render.</param>
        /// <returns>A <see cref="Task{HtmlString}"/> that on completion returns <see cref="HtmlString.Empty"/> that
        /// allows the <see cref="Write(object)"/> call to succeed.</returns>
        /// <remarks>The method writes to the <see cref="Output"/> and the value returned is a token
        /// value that allows the Write (produced due to @RenderSection(..)) to succeed. However the
        /// value does not represent the rendered content.</remarks>
        /// <exception cref="InvalidOperationException">if <paramref name="required"/> is <c>true</c> and the section
        /// was not registered using the <c>@section</c> in the Razor page.</exception>
        public async Task<HtmlString> RenderSectionAsync([NotNull] string name, bool required)
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

        /// <inheritdoc />
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