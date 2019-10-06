using ProjectIssuesSuite.API.data.Models;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ProjectIssuesSuite.API.data.Repositories
{
    public interface IVideoRepository
    {
        Task<bool> Exists(string fileName);
        Task<string> Upload(string fileNameWithExtension, Stream stream);
        Task<List<IListBlobItem>> GetList();
        void Delete(string fileName);
    }
}