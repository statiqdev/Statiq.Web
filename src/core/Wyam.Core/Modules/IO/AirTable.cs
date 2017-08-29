using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;

namespace Wyam.Core.Modules.IO
{
    /// <summary>
    /// Outputs new documents from the AirTable REST API. Input documents are ignored.
    /// </summary>
    /// <category>Input/Output</category>
    public class AirTable : ReadDataModule<AirTable, Dictionary<string, object>>
    {
        private readonly string _url;

        /// <summary>
        /// Outputs new documents from the AirTable API.
        /// </summary>
        /// <param name="url">The AirTable endpoint.</param>
        public AirTable(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new System.ArgumentException(nameof(url));
            }

            _url = url;
        }

        /// <inheritdoc />
        protected override IEnumerable<Dictionary<string, object>> GetItems(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            List<Dictionary<string, object>> items = new List<Dictionary<string, object>>();

            // Get the response
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return items;
            }
            string jsonString;
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                jsonString = reader.ReadToEnd();
            }

            // The JSON consists of
            // A "records" array containing one object per row
            // Each "record" object has three properties: "id", "fields", and "createdTime"
            // The "fields" property is an array of properties, named for the column name, containing the column value
            // Loop all the "records"
            foreach (JToken jsonRecord in JObject.Parse(jsonString)["records"])
            {
                Dictionary<string, object> fields = new Dictionary<string, object>
                {
                    // These are top level to the record
                    { "id", jsonRecord["id"] },
                    { "createdTime", jsonRecord["createdTime"] }
                };

                // Loop all the "fields"
                foreach (JProperty jsonField in jsonRecord["fields"].OfType<JProperty>())
                {
                    fields.Add(jsonField.Name, jsonField.Value);
                }
                items.Add(fields);
            }

            return items;
        }
    }
}