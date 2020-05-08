using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Logging;
using Polly;
using Statiq.Common;

namespace Statiq.Web.Azure
{
    /// <summary>
    /// Deploys an Azure Search Service index from the input document metadata.
    /// </summary>
    /// <category>Deployment</category>
    public class DeploySearchIndex : MultiConfigModule
    {
        private const int BatchSize = 1000;

        // Config keys
        private const string SearchServiceName = nameof(SearchServiceName);
        private const string IndexName = nameof(IndexName);
        private const string ApiKey = nameof(ApiKey);
        private const string Fields = nameof(Fields);

        /// <summary>
        /// Deploys an Azure Search Service index using data from the metadata of input documents.
        /// </summary>
        /// <param name="searchServiceName">The name of the search service.</param>
        /// <param name="indexName">The name of the search index.</param>
        /// <param name="apiKey">The API key used to authenticate.</param>
        /// <param name="fields">
        /// The search index fields. The field will be populated with the metadata value of the same name from each input document.
        /// </param>
        public DeploySearchIndex(
            Config<string> searchServiceName,
            Config<string> indexName,
            Config<string> apiKey,
            Config<IEnumerable<Field>> fields)
            : base(
                new Dictionary<string, IConfig>
                {
                    { SearchServiceName, searchServiceName.EnsureNonDocument(nameof(searchServiceName)) },
                    { IndexName, indexName.EnsureNonDocument(nameof(indexName)) },
                    { ApiKey, apiKey.EnsureNonDocument(nameof(apiKey)) },
                    { Fields, fields.EnsureNonDocument(nameof(fields)) }
                },
                false)
        {
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteConfigAsync(IDocument input, IExecutionContext context, IMetadata values)
        {
            string searchServiceName = values.GetString(SearchServiceName) ?? throw new ExecutionException("Invalid search service name");
            string indexName = values.GetString(IndexName) ?? throw new ExecutionException("Invalid search index name");
            string apiKey = values.GetString(ApiKey) ?? throw new ExecutionException("Invalid search API key");
            IList<Field> fields = values.GetList<Field>(Fields)?.ToList() ?? throw new ExecutionException("Invalid search fields");

            SearchServiceClient client = new SearchServiceClient(searchServiceName, new SearchCredentials(apiKey));

            // Delete the index if it currently exists (recreating is the easiest way to update it)
            CorsOptions corsOptions = null;
            if (await client.Indexes.ExistsAsync(indexName, null, context.CancellationToken))
            {
                // Get the CORS options because we'll need to recreate those
                Microsoft.Azure.Search.Models.Index existingIndex = await client.Indexes.GetAsync(indexName, null, context.CancellationToken);
                corsOptions = existingIndex.CorsOptions;

                // Delete the existing index
                context.LogDebug($"Deleting existing search index {indexName}");
                await client.Indexes.DeleteAsync(indexName, null, null, context.CancellationToken);
            }

            // Create the index
            Microsoft.Azure.Search.Models.Index index = new Microsoft.Azure.Search.Models.Index
            {
                Name = indexName,
                Fields = fields,
                CorsOptions = corsOptions
            };
            context.LogDebug($"Creating search index {indexName}");
            await client.Indexes.CreateAsync(index, null, context.CancellationToken);

            // Upload the documents to the search index in batches
            context.LogDebug($"Uploading {context.Inputs.Length} documents to search index {indexName}...");
            ISearchIndexClient indexClient = client.Indexes.GetClient(indexName);
            int start = 0;
            do
            {
                // Create the dynamic search documents and batch
                IndexAction<Microsoft.Azure.Search.Models.Document>[] indexActions = context.Inputs
                    .Skip(start)
                    .Take(BatchSize)
                    .Select(doc =>
                    {
                        Microsoft.Azure.Search.Models.Document searchDocument = new Microsoft.Azure.Search.Models.Document();
                        foreach (Field field in fields)
                        {
                            if (doc.ContainsKey(field.Name))
                            {
                                searchDocument[field.Name] = doc.Get(field.Name);
                            }
                        }
                        return IndexAction.Upload(searchDocument);
                    })
                    .ToArray();
                IndexBatch<Microsoft.Azure.Search.Models.Document> indexBatch = IndexBatch.New(indexActions);

                // Upload the batch with exponential retry for failures
                await Policy
                 .Handle<IndexBatchException>()
                 .WaitAndRetryAsync(
                    5,
                    attempt =>
                    {
                        context.LogWarning($"Failure while uploading batch {(start / BatchSize) + 1}, retry number {attempt}");
                        return TimeSpan.FromSeconds(Math.Pow(2, attempt));
                    },
                    (ex, _) => indexBatch = ((IndexBatchException)ex).FindFailedActionsToRetry(indexBatch, fields.Single(x => x.IsKey == true).Name))
                 .ExecuteAsync(async ct => await indexClient.Documents.IndexAsync(indexBatch, null, ct), context.CancellationToken);

                context.LogDebug($"Uploaded {start + indexActions.Length} documents to search index {indexName}");
                start += 1000;
            }
            while (start < context.Inputs.Length);

            return context.Inputs;
        }
    }
}
