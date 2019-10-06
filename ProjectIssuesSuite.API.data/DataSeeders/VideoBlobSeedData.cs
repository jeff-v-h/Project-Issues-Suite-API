using ProjectIssuesSuite.API.common.Models;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using System.Threading.Tasks;

namespace ProjectIssuesSuite.API.data.DataSeeders
{
    public class VideoBlobSeedData
    {
        private VideoStorageSettings _videoStorageSettings;
        private CloudStorageAccount _storageAccount;
        private NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public VideoBlobSeedData(IOptions<VideoStorageSettings> videoStorageSettings)
        {
            _videoStorageSettings = videoStorageSettings.Value;
        }

        public async Task InitContainer()
        {
            if (CloudStorageAccount.TryParse(_videoStorageSettings.ConnectionString, out _storageAccount))
            {
                try
                {
                    var client = _storageAccount.CreateCloudBlobClient();
                    var blobContainer = client.GetContainerReference(_videoStorageSettings.ContainerName);

                    await blobContainer.CreateIfNotExistsAsync();
                    _logger.Info($"Videos blob container either created or already exists.");
                }
                catch (StorageException ex)
                {
                    _logger.Error($"Error returned from the service: {ex}");
                }
            }
            else
            {
                _logger.Error($"Unable to parse blob video storage connection string");
            }
        }
    }
}
