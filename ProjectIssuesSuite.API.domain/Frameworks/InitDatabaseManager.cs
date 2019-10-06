using ProjectIssuesSuite.API.common.Models;
using ProjectIssuesSuite.API.data.DataSeeders;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace ProjectIssuesSuite.API.domain.Frameworks
{
    public class InitDatabaseManager
    {
        private ProjectSeedData _seedData { get; set; }
        private VideoBlobSeedData _blobSeedData { get; set; }

        public InitDatabaseManager(IOptions<DbSettings> dbSettings, IOptions<DbData> dbData, IOptions<VideoStorageSettings> videoStorageSettings)
        {
            _seedData = new ProjectSeedData(dbSettings, dbData);
            _blobSeedData = new VideoBlobSeedData(videoStorageSettings);
        }

        public async Task InitDatabase()
        {
            await _seedData.InitDbAndCollections();
            await _seedData.InitDocuments();

            await _blobSeedData.InitContainer();
        }
    }
}
