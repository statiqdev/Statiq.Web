using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Common.Shortcodes;
using Wyam.Core.Meta;
using Wyam.Core.Shortcodes;
using Wyam.Core.Util;

namespace Wyam.Core.Modules.Contents
{
    /// <summary>
    /// Renders shortcodes in the input documents.
    /// </summary>
    /// <category>Content</category>
    public class Shortcodes : IModule
    {
        private readonly string _startDelimiter;
        private readonly string _endDelimiter;

        /// <summary>
        /// Renders shortcodes in the input documents using the default start and end delimiters.
        /// </summary>
        /// <param name="preRender">
        /// Indicates if the module is being executed pre-template-rendering (<c>true</c>)
        /// or post-template-rendering (<c>false</c>). The default delimiters are different
        /// depending on when the module is executed.
        /// </param>
        public Shortcodes(bool preRender = false)
        {
            if (preRender)
            {
                _startDelimiter = ShortcodeParser.DefaultPreRenderStartDelimiter;
                _endDelimiter = ShortcodeParser.DefaultPreRenderEndDelimiter;
            }
            else
            {
                _startDelimiter = ShortcodeParser.DefaultPostRenderStartDelimiter;
                _endDelimiter = ShortcodeParser.DefaultPostRenderEndDelimiter;
            }
        }

        /// <summary>
        /// Renders shortcodes in the input documents using custom start and end delimiters.
        /// </summary>
        /// <param name="startDelimiter">The shortcode start delimiter.</param>
        /// <param name="endDelimiter">The shortcode end delimiter.</param>
        public Shortcodes(string startDelimiter, string endDelimiter)
        {
            _startDelimiter = startDelimiter;
            _endDelimiter = endDelimiter;
        }

        /// <inheritdoc />
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.AsParallel().Select(context, input =>
            {
                Stream stream = input.GetStream();
                if (ProcessShortcodes(stream, input, context, out IDocument result))
                {
                    return result;
                }
                stream.Dispose();
                return input;
            });
        }

        // The inputStream will be disposed if this returns <c>true</c> but will not otherwise
        private bool ProcessShortcodes(Stream inputStream, IDocument input, IExecutionContext context, out IDocument result)
        {
            // Parse the input stream looking for shortcodes
            ShortcodeParser parser = new ShortcodeParser(_startDelimiter, _endDelimiter, context.Shortcodes);
            if (!inputStream.CanSeek)
            {
                inputStream = new SeekableStream(inputStream, true);
            }
            List<ShortcodeLocation> locations = parser.Parse(inputStream);

            // Reset the position because we're going to use the stream again when we do replacements
            inputStream.Position = 0;

            // Return the original document if we didn't find any
            if (locations.Count == 0)
            {
                result = null;
                return false;
            }

            // Otherwise, create a shortcode instance for each named shortcode
            Dictionary<string, IShortcode> shortcodes =
                locations
                    .Select(x => x.Name)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(x => x, x => context.Shortcodes.CreateInstance(x), StringComparer.OrdinalIgnoreCase);

            // Execute each of the shortcodes in order
            List<InsertingStreamLocation> insertingLocations = locations
                .Select(x =>
                {
                    // Execute the shortcode
                    IShortcodeResult shortcodeResult = shortcodes[x.Name].Execute(x.Arguments, x.Content, input, context);

                    // Merge output metadata with the current input document
                    // Creating a new document is the easiest way to ensure all the metadata from shortcodes gets accumulated correctly
                    if (shortcodeResult?.Metadata != null)
                    {
                        input = context.GetDocument(input, shortcodeResult.Metadata);
                    }

                    // Recursively parse shortcodes
                    Stream shortcodeResultStream = shortcodeResult?.Stream;
                    if (shortcodeResultStream != null)
                    {
                        if (!shortcodeResultStream.CanSeek)
                        {
                            shortcodeResultStream = new SeekableStream(shortcodeResultStream, true);
                        }
                        if (ProcessShortcodes(shortcodeResultStream, input, context, out IDocument nestedResult))
                        {
                            input = nestedResult;
                            shortcodeResultStream = nestedResult.GetStream();  // Will get disposed in the replacement operation below
                        }
                        else
                        {
                            shortcodeResultStream.Position = 0;
                        }
                        return new InsertingStreamLocation(x.FirstIndex, x.LastIndex, shortcodeResultStream);
                    }

                    return new InsertingStreamLocation(x.FirstIndex, x.LastIndex, null);
                })
                .ToList();

            // Dispose any shortcodes that implement IDisposable
            foreach (IDisposable disposableShortcode
                in shortcodes.Values.Select(x => x as IDisposable).Where(x => x != null))
            {
                disposableShortcode.Dispose();
            }

            // Construct a new stream with the shortcode results inserted
            // We have to use all TextWriter/TextReaders over the streams to ensure consistent encoding
            Stream resultStream = context.GetContentStream();
            char[] buffer = new char[4096];
            using (TextWriter writer = new StreamWriter(resultStream, Encoding.UTF8, 4096, true))
            {
                // The input stream will get disposed when the reader is
                using (TextReader reader = new StreamReader(inputStream))
                {
                    int position = 0;
                    int length = 0;
                    foreach (InsertingStreamLocation insertingLocation in insertingLocations)
                    {
                        // Copy up to the start of this shortcode
                        length = insertingLocation.FirstIndex - position;
                        Read(reader, writer, length, ref buffer);
                        position += length;

                        // Copy the shortcode content to the result stream
                        if (insertingLocation.Stream != null)
                        {
                            // This will dispose the shortcode content stream when done
                            using (TextReader insertingReader = new StreamReader(insertingLocation.Stream))
                            {
                                Read(insertingReader, writer, null, ref buffer);
                            }
                        }

                        // Skip the shortcode text
                        length = insertingLocation.LastIndex - insertingLocation.FirstIndex + 1;
                        Read(reader, null, length, ref buffer);
                        position += length;
                    }

                    // Copy remaining
                    Read(reader, writer, null, ref buffer);
                }
            }
            result = context.GetDocument(input, resultStream);
            return true;
        }

        // writer = null to just skip length in reader
        // length = null to read to the end of reader
        private static void Read(TextReader reader, TextWriter writer, int? length, ref char[] buffer)
        {
            while (!length.HasValue || length > 0)
            {
                int count = reader.ReadBlock(buffer, 0, !length.HasValue || length > buffer.Length ? buffer.Length : length.Value);
                if (count > 0)
                {
                    if (length.HasValue)
                    {
                        length -= count;
                    }
                    writer?.Write(buffer, 0, count);
                    writer?.Flush();
                }
                else
                {
                    break;
                }
            }
        }

        private class InsertingStreamLocation
        {
            public InsertingStreamLocation(int firstIndex, int lastIndex, Stream stream)
            {
                FirstIndex = firstIndex;
                LastIndex = lastIndex;
                Stream = stream;
            }

            public int FirstIndex { get; }
            public int LastIndex { get; }
            public Stream Stream { get; }
        }
    }
}
