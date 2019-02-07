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
    public class Shortcodes : IModule
    {
        private readonly string _startDelimiter = ShortcodeParser.DefaultStartDelimiter;
        private readonly string _endDelimiter = ShortcodeParser.DefaultEndDelimiter;

        public Shortcodes()
        {
        }

        public Shortcodes(string startDelimiter, string endDelimiter)
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

                // Construct a new stream with the shortcodes inserted
                Stream resultStream = context.GetContentStream();
                char[] buffer = new char[4096];
                using (Stream inputStream = input.GetStream())
                {
                    using (TextWriter writer = new StreamWriter(resultStream))
                    {
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
                                insertingLocation.Stream.CopyTo(resultStream);

                                // Skip the shortcode text
                                length = insertingLocation.LastIndex - insertingLocation.FirstIndex + 1;
                                Read(reader, null, length, ref buffer);
                                position += length;
                            }
                        }
                    }

                    // Copy remaining
                    inputStream.CopyTo(resultStream);
                }

                return context.GetDocument(input, resultStream);
            });
        }

        private static void Read(TextReader reader, TextWriter writer, int length, ref char[] buffer)
        {
            while (length > 0)
            {
                int count = reader.ReadBlock(buffer, 0, length > buffer.Length ? buffer.Length : length);
                if (count > 0)
                {
                    length -= count;
                    writer?.Write(buffer, 0, count);
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
