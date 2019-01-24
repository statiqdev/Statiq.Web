using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
            List<string> result = new List<string>();

            JsonSerializerSettings jsonSettings = new JsonSerializerSettings
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
                object value = (parameters.Length >= 1) ? parameters[0] : (object)context;

                string json = Json(value, out string[] errors);
                writer.WriteSafeString(json);

                WriteErrors(writer, parameters, errors);
            });

            HDN.Handlebars.RegisterHelper("yaml", (writer, context, parameters) =>
            {
                object value = (parameters.Length >= 1) ? parameters[0] : (object)context;
                string json = Json(value, out string[] errors);
                YamlDotNet.Serialization.Deserializer reader = new YamlDotNet.Serialization.DeserializerBuilder()
                    .Build();

                object obj = reader.Deserialize<object>(json);

                YamlDotNet.Serialization.Serializer serializer = new YamlDotNet.Serialization.SerializerBuilder()
                    .Build();
                string yaml = serializer.Serialize(obj);
                writer.WriteSafeString(yaml);

                WriteErrors(writer, parameters, errors);
            });
        }

        private static void WriteErrors(TextWriter writer, object[] parameters, string[] errors)
        {
            bool writeErrors = (parameters.Length >= 2 && bool.TryParse(parameters[1].ToString(), out bool parsed)) ? parsed : false;
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
                    Func<object, string> template = HDN.Handlebars.Compile(input.Content);
                    Dictionary<string, object> metadata = new Dictionary<string, object>();
                    foreach (KeyValuePair<string, object> meta in input.Metadata)
                    {
                        metadata[meta.Key] = meta.Value;
                    }

                    var templateValues = new { metadata, content = input.Content };
                    string output = template(templateValues);
                    MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(output));
                    IDocument document = context.GetDocument(input, stream);
                    return document;
                })
                .Where(x => x != null);
        }
    }
}
