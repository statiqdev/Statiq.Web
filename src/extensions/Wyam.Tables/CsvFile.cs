using System.Collections.Generic;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;

namespace Wyam.Tables
{
    internal static class CsvFile
    {
        public static IEnumerable<IEnumerable<string>> GetAllRecords(Stream stream, string delimiter = null)
        {
            using (var reader = new StreamReader(stream))
            {
                return GetAllRecords(reader, delimiter);
            }
        }

        public static IEnumerable<IEnumerable<string>> GetAllRecords(TextReader reader, string delimiter = null)
        {
            List<IEnumerable<string>> records = new List<IEnumerable<string>>();
            var configuration = delimiter == null ? new Configuration { HasHeaderRecord = false } : new Configuration { HasHeaderRecord = false, Delimiter = delimiter };

            using (var csv = new CsvReader(reader, configuration))
            {
                while (csv.Read())
                {
                    var currentRecord = csv.Context.Record;
                    records.Add(currentRecord);
                }
            }

            return records;
        }

        public static void WriteAllRecords(IEnumerable<IEnumerable<string>> records, Stream stream)
        {
            var writer = new StreamWriter(stream);
            WriteAllRecords(records, writer);
            writer.Flush();
        }

        public static void WriteAllRecords(IEnumerable<IEnumerable<string>> records, TextWriter writer)
        {
            if (records == null)
            {
                return;
            }

            var csv = new CsvWriter(writer, new Configuration { QuoteAllFields = true });
            {
                foreach (var row in records)
                {
                    foreach (var cell in row)
                    {
                        csv.WriteField(cell ?? string.Empty);
                    }
                    csv.NextRecord();
                }
            }
        }
    }
}
