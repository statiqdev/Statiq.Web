using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Microsoft.Extensions.Logging;
using Polly;
using Statiq.Common;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;

namespace Statiq.Web.Azure
{
    /// <summary>
    /// Deploys an Azure Search Service index from the input document metadata.
    /// </summary>
    /// <category name="Deployment" />
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
            Config<IEnumerable<SearchField>> fields)
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
            IList<SearchField> fields = values.GetList<SearchField>(Fields)?.ToList() ?? throw new ExecutionException("Invalid search fields");

            if (!Uri.TryCreate(searchServiceName, UriKind.Absolute, out Uri searchServiceUri))
            {
                throw new ExecutionException("Invalid search service name");
            }

            SearchIndexClient searchIndexClient = new SearchIndexClient(searchServiceUri, new AzureKeyCredential(apiKey));

            // Create the index
            SearchIndex index = new SearchIndex(indexName)
            {
                Fields = fields,
            };
            context.LogDebug($"Creating search index {indexName}");
            await searchIndexClient.CreateOrUpdateIndexAsync(index, true, true, context.CancellationToken);

            // Upload the documents to the search index in batches
            context.LogDebug($"Uploading {context.Inputs.Length} documents to search index {indexName}...");
            SearchClient indexClient = searchIndexClient.GetSearchClient(indexName);
            int start = 0;
            do
            {
                // Create the dynamic search documents and batch
                IndexDocumentsAction<SearchDocument>[] indexActions = context.Inputs
                    .Skip(start)
                    .Take(BatchSize)
                    .Select(doc =>
                    {
                        SearchDocument searchDocument = new SearchDocument();
                        foreach (SearchField field in fields)
                        {
                            if (doc.ContainsKey(field.Name))
                            {
                                searchDocument[field.Name] = doc.Get(field.Name);
                            }
                        }

                        return IndexDocumentsAction.Upload(searchDocument);
                    })
                    .ToArray();
                IndexDocumentsBatch<SearchDocument> indexBatch = IndexDocumentsBatch.Create(indexActions);

                // Upload the batch with exponential retry for failures
                await Policy
                    .Handle<RequestFailedException>()
                    .OrResult<IndexDocumentsResult>(r => r.Results.Any(result => !result.Succeeded))
                    .WaitAndRetryAsync(
                        5,
                        attempt =>
                        {
                            context.LogWarning($"Failure while uploading batch {(start / BatchSize) + 1}, retry number {attempt}");
                            return TimeSpan.FromSeconds(Math.Pow(2, attempt));
                        },
                        (ex, _) =>
                        {
                            IEnumerable<string> failedResults = ex.Result.Results.Where(r => !r.Succeeded).Select(result => result.Key);
                            IEnumerable<IndexDocumentsAction<SearchDocument>> failedActions = indexActions.Where(action => action.Document.Keys.Any(key => failedResults.Contains(key)));
                            indexBatch = IndexDocumentsBatch.Create(failedActions.ToArray());
                        })
                    .ExecuteAsync(async ct => await indexClient.IndexDocumentsAsync(indexBatch, null, ct), context.CancellationToken);

                context.LogDebug($"Uploaded {start + indexActions.Length} documents to search index {indexName}");
                start += 1000;
            }
            while (start < context.Inputs.Length);

            return context.Inputs;
        }
    }
}
