using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Modules.Json
{
    public class Json : IModule
    {
        private readonly bool _flatten;
        private readonly string _key;

        public Json()
        {
            _flatten = true;
        }

        public Json(string key, bool flatten = false)
        {
            _key = key;
            _flatten = flatten;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs
                .AsParallel()
                .Select(x =>
                {
                    try
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        Dictionary<string, object> items = new Dictionary<string, object>();
                        ExpandoObject json;
                        using (TextReader contentReader = new StringReader(x.Content))
                        {
                            using (JsonReader jsonReader = new JsonTextReader(contentReader))
                            {
                                json = serializer.Deserialize<ExpandoObject>(jsonReader);
                            }
                        }
                        if (!string.IsNullOrEmpty(_key))
                        {
                            items[_key] = json;
                        }
                        if (_flatten)
                        {
                            foreach (KeyValuePair<string, object> item in json)
                            {
                                items[item.Key] = item.Value;
                            }
                        }
                        return x.Clone(items);
                    }
                    catch (Exception ex)
                    {
                        context.Trace.Error("Error processing JSON for {0}: {1}", x.Source, ex.ToString());
                    }
                    return null;
                })
                .Where(x => x != null);
        }
    }
}
