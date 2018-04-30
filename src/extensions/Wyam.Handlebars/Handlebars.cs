using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HandlebarsDotNet;
using Newtonsoft.Json;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using HDN = HandlebarsDotNet;

namespace Wyam.Handlebars
{
    public class Handlebars : IModule
    {
        private static string Json(object value, out string[] errors)
        {
            var result = new List<string>();

            var jsonSettings = new JsonSerializerSettings
            {
                Error = (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args) =>
                {
                    result.Add($"{args.ErrorContext.Path} : {args.ErrorContext.Error.Message}");
                    args.ErrorContext.Handled = true;
                },
                Formatting = Formatting.Indented,
            };

            string json = JsonConvert.SerializeObject(value, jsonSettings);
            errors = result.ToArray();
            return json;
        }

        static Handlebars()
        {
            HDN.Handlebars.RegisterHelper("json", (writer, context, parameters) =>
            {
                var value = (parameters.Length >= 1) ? parameters[0] : (object)context;

                var json = Json(value, out var errors);
                writer.WriteSafeString(json);

                WriteErrors(writer, parameters, errors);
            });

            HDN.Handlebars.RegisterHelper("yaml", (writer, context, parameters) =>
            {
                var value = (parameters.Length >= 1) ? parameters[0] : (object)context;
                var json = Json(value, out var errors);
                var reader = new YamlDotNet.Serialization.DeserializerBuilder()
                    .Build();

                var obj = reader.Deserialize<object>(json);

                var serializer = new YamlDotNet.Serialization.SerializerBuilder()
                    .Build();
                string yaml = serializer.Serialize(obj);
                writer.WriteSafeString(yaml);

                WriteErrors(writer, parameters, errors);
            });
        }

        private static void WriteErrors(TextWriter writer, object[] parameters, string[] errors)
        {
            var writeErrors = (parameters.Length >= 2 && bool.TryParse(parameters[1].ToString(), out var parsed)) ? parsed : false;
            if (writeErrors && errors.Length != 0)
            {
                writer.WriteSafeString(
                    Environment.NewLine + "Serialisation errors :" + Environment.NewLine + "- "
                    + string.Join(Environment.NewLine + "- ", errors));
            }
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs
                .AsParallel()
                .Select(context, input =>
                {
                    var template = HDN.Handlebars.Compile(input.Content);
                    var metadata = new Dictionary<string, object>();
                    foreach (var meta in input.Metadata) metadata[meta.Key] = meta.Value;
                    var templateValues = new { metadata, content = input.Content };
                    var output = template(templateValues);
                    var stream = new MemoryStream(Encoding.UTF8.GetBytes(output));
                    var document = context.GetDocument(input, stream);
                    return document;
                })
                .Where(x => x != null);
        }
    }
}
