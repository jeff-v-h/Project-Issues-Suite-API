using ProjectIssuesSuite.API.common.Models;
using ProjectIssuesSuite.API.data.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ProjectIssuesSuite.API.data.Repositories
{
    public class CosmosRepository<T> : IDocumentRepository<T> where T : DocumentBase
    {
        private IDocumentClient _client { get; set; }
        private DbSettings _dbSettings;
        private readonly ILogger<CosmosRepository<T>> _logger;
        private string _dbName { get; set; }
        private string _collectionName { get; set; }

        public CosmosRepository(IOptions<DbSettings> dbSettings, ILogger<CosmosRepository<T>> logger, IDocumentClient client)
        {
            _dbSettings = dbSettings.Value;
            _client = client;
            _logger = logger;
            _dbName = _dbSettings.DbName;
            _collectionName = typeof(T).Name + "s";
        }

        public async Task<T> Create(T document)
        {
            Document doc = await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_dbName, _collectionName), document);
            _logger.LogInformation($"Document in collection '{_collectionName}' has been CREATED. The DB has given it an id of '{doc.Id}'.");
            document.Id = doc.Id;
            return document;
        }

        public IQueryable<T> GetAll()
        {
            // set feed options to pass into document query.
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            // Find a document collection via its Name
            IQueryable<T> query = _client.CreateDocumentQuery<T>(
                    UriFactory.CreateDocumentCollectionUri(_dbName, _collectionName), queryOptions);

            return query;
        }

        public IQueryable<T> GetBy(Expression<Func<T, bool>> predicate, int MaxItemCount = -1)
        {
            // set feed options to pass into document query.
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = MaxItemCount };

            // Find a document collection via its Name
            IQueryable<T> query = _client.CreateDocumentQuery<T>(
                    UriFactory.CreateDocumentCollectionUri(_dbName, _collectionName), queryOptions)
                    .Where(predicate);

            return query;
        }

        public T GetFirstOrDefault(Expression<Func<T, bool>> predicate)
        {
            return GetBy(predicate, 1).ToList().FirstOrDefault();
        }

        public async Task Update(T updatedDoc)
        {
            await _client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(_dbName, _collectionName, updatedDoc.Id), updatedDoc);
            _logger.LogInformation($"Document of type '{typeof(T).Name}' with id '{updatedDoc.Id}' has been UPDATED");
        }

        public async Task Delete(string id)
        {
            await _client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(_dbName, _collectionName, id));
            _logger.LogInformation($"Document of type '{typeof(T).Name}' with id '{id}' has been DELETED");
        }
    }
}
