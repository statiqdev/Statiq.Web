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

namespace Wyam.Core.Modules.Contents
{
    public class ProcessShortcodes : IModule
    {
        private readonly string _startDelimiter = ShortcodeParser.DefaultStartDelimiter;
        private readonly string _endDelimiter = ShortcodeParser.DefaultEndDelimiter;

        public ProcessShortcodes()
        {
        }

        public ProcessShortcodes(string startDelimiter, string endDelimiter)
        {
            _startDelimiter = startDelimiter;
            _endDelimiter = endDelimiter;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.AsParallel().Select(context, input =>
            {
                // Parse the input stream looking for shortcodes
                ShortcodeParser parser = new ShortcodeParser(_startDelimiter, _endDelimiter, context.Shortcodes);
                List<ShortcodeLocation> locations;
                using (Stream inputStream = input.GetStream())
                {
                    locations = parser.Parse(inputStream);
                }

                // Return the original document if we didn't find any
                if (locations.Count == 0)
                {
                    return input;
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
                        IShortcodeResult result = shortcodes[x.Name].Execute(x.Arguments, x.Content, input, context);
                        if (result.Metadata != null)
                        {
                            // Creating a new document is the easiest way to ensure all the metadata from shortcodes gets accumulated correctly
                            input = context.GetDocument(input, result.Metadata);
                        }
                        return new InsertingStreamLocation(x.FirstIndex, x.LastIndex, result.Stream);
                    })
                    .ToList();

                // Dispose any shortcodes that implement IDisposable
                foreach (IDisposable disposableShortcode
                    in shortcodes.Values.Select(x => x as IDisposable).Where(x => x != null))
                {
                    disposableShortcode.Dispose();
                }

                // Construct a new stream with the shortcodes inserted
                // We have to use all TextWriter/TextReaders over the streams to ensure consistent encoding
                Stream resultStream = context.GetContentStream();
                char[] buffer = new char[4096];
                using (TextWriter writer = new StreamWriter(resultStream, Encoding.UTF8, 4096, true))
                {
                    // The input stream will get disposed when the reader is
                    Stream inputStream = input.GetStream();
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

                return context.GetDocument(input, resultStream);
            });
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

        public class InsertingStreamLocation
        {
            public InsertingStreamLocation(int firstIndex, int lastIndex, Stream stream)
            {
                FirstIndex = firstIndex;
                LastIndex = lastIndex;
                Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            }

            public int FirstIndex { get; }
            public int LastIndex { get; }
            public Stream Stream { get; }
        }
    }

    // TODO: test the metadata accumulates over multiple shortcodes
    // TODO: test that document metadata is available to shortcode
}
