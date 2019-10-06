using ProjectIssuesSuite.API.common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ProjectIssuesSuite.API.data.Repositories
{
    public class VideoRepository : IVideoRepository
    {
        private VideoStorageSettings _videoStorageSettings;
        private CloudStorageAccount _storageAccount;
        private CloudBlobClient _client;
        private CloudBlobContainer _blobContainer;
        private readonly ILogger<VideoRepository> _logger;

        public VideoRepository(IOptions<VideoStorageSettings> videoStorageSettings, ILogger<VideoRepository> logger)
        {
            _logger = logger;
            _videoStorageSettings = videoStorageSettings.Value;
            if (CloudStorageAccount.TryParse(_videoStorageSettings.ConnectionString, out _storageAccount))
            {
                try
                {
                    _client = _storageAccount.CreateCloudBlobClient();
                    _blobContainer = _client.GetContainerReference(_videoStorageSettings.ContainerName);

                    // set permissions so the blobs are public
                    BlobContainerPermissions permissions = new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    };
                    _blobContainer.SetPermissionsAsync(permissions).Wait();
                }
                catch (StorageException ex)
                {
                    _logger.LogError($"Error returned from the service: {ex}");
                }
            }
            else
            {
                _logger.LogError($"Unable to parse blob video storage connection string");
            }
        }

        public async Task<bool> Exists(string fileName)
        {
            CloudBlockBlob blockBlob = _blobContainer.GetBlockBlobReference(fileName);

            return await blockBlob.ExistsAsync();
        }

        public async Task<string> Upload(string fileNameWithExtension, Stream stream)
        {
            CloudBlockBlob blockBlob = _blobContainer.GetBlockBlobReference(fileNameWithExtension);

            await blockBlob.UploadFromStreamAsync(stream);
            _logger.LogInformation($"File uploaded: {fileNameWithExtension}");

            return _videoStorageSettings.EndpointUri + '/' + fileNameWithExtension;
        }

        public async Task<List<IListBlobItem>> GetList()
        {
            var list = new List<IListBlobItem>();
            BlobContinuationToken token = null;

            do
            {
                var resultSegment = await _blobContainer.ListBlobsSegmentedAsync(null, token);
                token = resultSegment.ContinuationToken;

                foreach (IListBlobItem item in resultSegment.Results)
                {
                    list.Add(item);
                }
            } while (token != null);

            return list;
        }

        public void Delete(string fileName)
        {
            CloudBlockBlob blockBlob = _blobContainer.GetBlockBlobReference(fileName);
            blockBlob.DeleteIfExistsAsync();
            _logger.LogInformation($"File deleted: {fileName}");
        }
    }
}
