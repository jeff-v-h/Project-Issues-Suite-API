using ProjectIssuesSuite.API.data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectIssuesSuite.API.domain.Managers
{
    public interface IVideoManager
    {
        Task<List<Video>> Upload(IFormFileCollection videoFiles, ICollection<string> thumbnails);
        Task<List<IListBlobItem>> GetList();
        void Delete(string fileLocation);
    }
}
