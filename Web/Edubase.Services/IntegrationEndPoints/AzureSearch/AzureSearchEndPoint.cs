﻿using Edubase.Common;
using Edubase.Common.Reflection;
using Edubase.Services.Establishments.Models;
using Edubase.Services.Exceptions;
using Edubase.Services.IntegrationEndPoints.AzureSearch.Models;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Rest.Azure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Edubase.Services.IntegrationEndPoints.AzureSearch
{
    public class AzureSearchEndPoint : IAzureSearchEndPoint
    {
        public const string ODATA_FILTER_DELETED = "IsDeleted eq false";
        private string _connectionString;
        private Dictionary<Type, IList<string>> _fieldLists = new Dictionary<Type, IList<string>>();
        
        public struct ConnectionString
        {
            public string ApiKey { get; set; }
            public string Name { get; set; }
            public static ConnectionString Parse(string text)
            {
                var parts = text.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => new Tuple<string, string>(x.GetPart("=", 0), x.GetPart("=", 1)));

                var retVal = new ConnectionString();
                retVal.Name = parts.FirstOrDefault(x => x.Item1.Equals("name", StringComparison.OrdinalIgnoreCase))?.Item2;
                retVal.ApiKey = parts.FirstOrDefault(x => x.Item1.Equals("apikey", StringComparison.OrdinalIgnoreCase))?.Item2;
                return retVal;
            }
        }

        private ConnectionString _connection;


        public AzureSearchEndPoint(string connectionString)
        {
            _connectionString = connectionString;
            _connection = ConnectionString.Parse(_connectionString);
        }
        
        public async Task DeleteIndexAsync(string name) => await GetClient().Indexes.DeleteAsync(name.ToLower());
        public async Task DeleteIndexerAsync(string name) => await GetClient().Indexers.DeleteAsync(name.ToLower());
        public async Task DeleteDataSourceAsync(string name) => await GetClient().DataSources.DeleteAsync(name.ToLower());
        public async Task CreateOrUpdateDataSource(string name, string sqlConnectionString, string tableName, string description = null)
        {
            var c = GetClient();
            await c.DataSources.CreateOrUpdateAsync(DataSource.AzureSql(name, sqlConnectionString, tableName, 
                new SqlIntegratedChangeTrackingPolicy(), description));
        }
        public async Task CreateOrUpdateIndexerAsync(string name, string dataSourceName, string targetIndexName, string description = null)
        {
            var c = GetClient();
            await c.Indexers.CreateOrUpdateAsync(new Indexer()
            {
                DataSourceName = dataSourceName,
                Name = name,
                TargetIndexName = targetIndexName,
                Description = description
            });
        }
        public async Task CreateOrUpdateIndexAsync(string name, IList<SearchIndexField> fields, string suggesterName = null)
        {
            name = name.ToLower();

            var suggesters = new List<Suggester>();
            if (suggesterName != null && fields.Any(x => x.IncludeInSuggester == true))
                suggesters.Add(new Suggester(suggesterName, SuggesterSearchMode.AnalyzingInfixMatching, fields.Where(x => x.IncludeInSuggester == true).Select(x => x.Name).ToArray()));

            var client = GetClient();
            await client.Indexes.CreateOrUpdateAsync(new Index(name, fields.Cast<Field>().ToList(), suggesters: suggesters));
        }

        public async Task<ApiSearchResult<T>> SearchAsync<T>(string indexName, string text = null, string filter = null, int skip = 0, int take = 10, IList<string> fullTextSearchFields = null, IList<string> orderBy = null) where T : class
        {
            try
            {
                if (skip > 100000) throw new Exception($"The skip parameter cannot be greater than 100,000");
                if (take == 0) throw new Exception($"Argument {nameof(take)} cannot be zero.");
                var fields = _fieldLists.Get(typeof(T), () => ReflectionHelper.GetProperties(typeof(T), typeof(AZSIgnoreAttribute), true));

                var result = await GetIndexClient(indexName).Documents.SearchAsync<T>(text, new SearchParameters()
                {
                    Skip = skip,
                    Top = take,
                    SearchMode = SearchMode.All,
                    Filter = filter,
                    OrderBy = orderBy,
                    IncludeTotalResultCount = (skip == 0),
                    Select = fields,
                    SearchFields = fullTextSearchFields
                });

                var retVal = new ApiSearchResult<T>(result);
                return retVal;
            }
            catch (CloudException ex) when (ex.Message.Contains("The filter expression has too many clauses"))
            {
                throw new SearchQueryTooLargeException("The search query is too large/complex. Please reduce the complexity/size and try again.", ex);
            }
        }
        

        public async Task CreateOrUpdateIndexAsync(SearchIndex index) => await CreateOrUpdateIndexAsync(index.Name, index.Fields, index.SuggesterName);


        public async Task<IEnumerable<T>> SuggestAsync<T>(string indexName, string suggesterName, string text, string odataFilter = null, int take = 5) where T : class
        {
            var fields = _fieldLists.Get(typeof(T), () => ReflectionHelper.GetProperties(typeof(T), writeableOnly: true));
            
            var c = GetIndexClient(indexName);
            var result = await c.Documents.SuggestAsync<T>(text, suggesterName, new SuggestParameters() { Select = fields, Top = take, Filter = odataFilter ?? ODATA_FILTER_DELETED });
            return result.Results.Select(x => x.Document);
        }

        public async Task<Tuple<string, long>> GetStatusAsync(string indexName)
        {
            var c = GetClient();
            
            var documentCount = (await c.Indexes.ExistsAsync(indexName)) 
                ? (await c.Indexes.GetStatisticsAsync(indexName))?.DocumentCount : 0;

            var status = (await c.Indexers.ExistsAsync(indexName + "-indexer"))
                ? (await c.Indexers.GetStatusAsync(indexName + "-indexer"))?.LastResult?.Status.ToString() ?? "indexernotfound"
                : "indexernotfound";

            return new Tuple<string, long>(status, documentCount.GetValueOrDefault());
        }

        
        private SearchServiceClient GetClient() => new SearchServiceClient(_connection.Name, new SearchCredentials(_connection.ApiKey));

        private SearchIndexClient GetIndexClient(string indexName) => new SearchIndexClient(_connection.Name, indexName, new SearchCredentials(_connection.ApiKey));

    }
}
