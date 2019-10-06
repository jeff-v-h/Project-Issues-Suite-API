using ProjectIssuesSuite.API.data.Models;
using ProjectIssuesSuite.API.data.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectIssuesSuite.API.domain.Managers
{
    public class VideoManager : IVideoManager
    {
        private readonly ILogger<VideoManager> _logger;
        private IVideoRepository _videoRepo;

        public VideoManager(ILogger<VideoManager> logger, IVideoRepository videoRepo)
        {
            _logger = logger;
            _videoRepo = videoRepo;
        }

        public async Task<List<Video>> Upload(IFormFileCollection videoFiles, ICollection<string> thumbnails)
        {
            var videoList = new List<Video>();
            var thumbs = thumbnails.ToArray();

            // Upload each file
            for (int i = 0; i < videoFiles.Count; i++)
            {
                var video = videoFiles[i];

                // Create full filename with id and extension as well
                var extension = '.' + video.ContentType.Split('/').Last();
                var fileNameNoSpace = video.FileName.Replace(' ', '-');
                string newId;
                string fullFileName;

                // Make sure the video name does not exist, otherwise it will be overwritten
                do
                {
                    newId = Guid.NewGuid().ToString();
                    fullFileName = newId + '.' + fileNameNoSpace + extension;
                } while (await _videoRepo.Exists(fullFileName));

                // Convert blob to stream and upload
                var stream = video.OpenReadStream();
                var fileLocation = await _videoRepo.Upload(fullFileName, stream);

                // Create a new Video to add to the list of metadata that will be returned
                videoList.Add(new Video
                {
                    Id = newId,
                    Title = video.FileName,
                    FileLocation = fileLocation,
                    Thumbnail = thumbs[i]
                });
            }

            return videoList;
        }

        public async Task<List<IListBlobItem>> GetList()
        {
            return await _videoRepo.GetList();
        }

        public void Delete(string fileLocation)
        {
            // Remove trailing forward slash if there is one
            if (fileLocation.EndsWith('/'))
            {
                fileLocation = fileLocation.Remove(fileLocation.Length - 1);
            }
            var fileName = fileLocation.Split('/').Last();

            _videoRepo.Delete(fileName);
        }
    }
}
