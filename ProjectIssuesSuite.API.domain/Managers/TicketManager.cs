using ProjectIssuesSuite.API.data.Models;
using ProjectIssuesSuite.API.data.Repositories;
using ProjectIssuesSuite.API.domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectIssuesSuite.API.domain.Managers
{
    public class TicketManager : ITicketManager
    {
        private IProjectRepository _projectRepo;
        private ITicketRepository _ticketRepo;
        private IVideoManager _videoManager;
        private readonly ILogger<TicketManager> _logger;

        public TicketManager(ILogger<TicketManager> logger, IProjectRepository projectRepo,
            ITicketRepository ticketRepo, IVideoManager videoManager)
        {
            _logger = logger;
            _ticketRepo = ticketRepo;
            _projectRepo = projectRepo;
            _videoManager = videoManager;
        }

        public ICollection<TicketViewModel> GetAllTickets()
        {
            var tickets = _ticketRepo.GetAll().ToList();

            if (tickets.Count() < 1)
            {
                _logger.LogInformation("No projects were were returned from DB.");
            }

            var ticketVMs = new List<TicketViewModel>();
            foreach (Ticket ticket in tickets)
            {
                ticketVMs.Add(new TicketViewModel(ticket));
            }

            return ticketVMs;
        }

        public TicketViewModel GetTicket(string ticketId)
        {
            var ticket = _ticketRepo.GetById(ticketId);

            if (ticket == null)
            {
                return null;
            }

            return new TicketViewModel(ticket);
        }

        public TicketViewModel GetTicketByName(string projectName, string ticketName)
        {
            var project = _projectRepo.GetByName(projectName);
            if (project == null) return null;

            // Find the ticket reference within the project
            var ticketBase = project.Tickets.FirstOrDefault(ticketRef => ticketRef.Name.ToUpper() == ticketName.ToUpper());
            if (ticketBase == null) return null;

            // Then use the id to get it from the ticket collection
            var ticket = _ticketRepo.GetById(ticketBase.Id);
            if (ticket == null) return null;

            return new TicketViewModel(ticket);
        }

        public async Task<TicketViewModel> CreateTicket(TicketViewModel newTicketVM)
        {
            // Ensure the project name exists before creating anything.
            var projectToUpdate = _projectRepo.GetByName(newTicketVM.ProjectName);
            if (projectToUpdate == null)
            {
                _logger.LogError($"\tThe Ticket's project '{newTicketVM.ProjectName}' could not be found in the DB");
                return null;
            }
            string log = "";

            // Pass the new ticket into the database and return the newly created id
            var newTicket = new Ticket()
            {
                Name = newTicketVM.Name,
                Description = newTicketVM.Description,
                ProjectName = newTicketVM.ProjectName,
                Status = "open",
                Creator = newTicketVM.Creator
            };

            // If there is at least one video file attached, pass through to video manager to upload
            var videoFiles = newTicketVM.VideoFiles;
            if (videoFiles != null && videoFiles.Count > 0)
            {
                // Upload the videos via VideoManager and return video meta data 
                newTicket.Videos = await _videoManager.Upload(videoFiles, newTicketVM.VideoThumbnails);

                foreach (var videoData in newTicket.Videos) { log += $"Video '{videoData.Title}' uploaded. "; }

                // Remove the videoFiles since they have already been uploaded
                newTicketVM.VideoFiles = null;
                newTicketVM.VideoThumbnails = null;
            }

            newTicket.EventLog = new List<Log>
            {
                new Log
                {
                    DateAndTime = DateTime.Now,
                    Event = log += "Ticket created."
                }
            };

            // create the ticket in the db and pass the new id into the view model
            await _ticketRepo.Create(newTicket);
            newTicketVM.Id = newTicket.Id;
            newTicketVM.Videos = newTicket.Videos;
            newTicketVM.Status = newTicket.Status;
            newTicketVM.EventLog = newTicket.EventLog;

            // Update the project in the DB with the created ticket's id and name
            UpsertTicketBaseToProject(projectToUpdate, newTicket.Id, newTicket.Name);

            await _projectRepo.Update(projectToUpdate);

            // Transfer properties to a ViewModel to return back to the controller
            return newTicketVM;
        }

        public async Task<bool> ReplaceTicket(TicketViewModel newTicketVM)
        {
            // Get the Ticket and ensure it exists
            var ticketToUpdate = _ticketRepo.GetById(newTicketVM.Id);
            if (ticketToUpdate == null)
            {
                _logger.LogError($"\tTicket with id {newTicketVM.Id} was not found in the DB. Nothing was updated.");
                return false;
            }

            // string to add log message to
            string log = "";

            // If there is at least one video file attached, pass through to video manager to upload
            var videoFiles = newTicketVM.VideoFiles;
            if (videoFiles != null && videoFiles.Count > 0)
            {
                // Upload the videos via VideoManager and return meta data for videos
                var videos = await _videoManager.Upload(videoFiles, newTicketVM.VideoThumbnails);

                foreach (var video in videos)
                {
                    ticketToUpdate.Videos.Add(video);
                    log += $"Video '{video.Title}' uploaded. ";
                }

                // Remove the videoFiles since they have already been uploaded
                newTicketVM.VideoFiles = null;
            }
            else // The following are only done for actions that arent actioned with adding a video
            {
                var newVideoData = newTicketVM.Videos;
                var oldVideoData = ticketToUpdate.Videos;

                // When video/s being removed
                if (newVideoData.Count < oldVideoData.Count)
                {
                    // Delete them from the video storage
                    foreach (var video in oldVideoData)
                    {
                        var potentialVid = newVideoData.FirstOrDefault(x => x.Id == video.Id);

                        if (potentialVid == null)
                        {
                            _videoManager.Delete(video.FileLocation);
                            log += $"Video '{video.Title}' removed. ";
                        }
                    }
                }

                // When no video/s removed, check for changes in notes before the metadata is copied over
                if (newVideoData.Count == oldVideoData.Count)
                {
                    for (int i = 0; i < newVideoData.Count; i++)
                    {
                        var newNotesCount = newVideoData.ToList()[i].Notes.Count;
                        var oldNotesCount = oldVideoData.ToList()[i].Notes.Count;

                        if (newNotesCount != oldNotesCount)
                        {
                            log += $"Number of notes changed from {oldNotesCount} to {newNotesCount} for video '{newVideoData.ToList()[i].Title}'. ";
                        }
                    }
                }

                // Pass the Video metadata since current videos' titles can be changed or notes can be added
                ticketToUpdate.Videos = newTicketVM.Videos;
            }

            // Update values that have been changed
            if (ticketToUpdate.Description != newTicketVM.Description)
            {
                log += $"Description changed from '{ticketToUpdate.Description}' to '{newTicketVM.Description}'. ";
                ticketToUpdate.Description = newTicketVM.Description;
            }

            if (ticketToUpdate.Status != newTicketVM.Status)
            {
                log += $"Status changed from '{ticketToUpdate.Status}' to '{newTicketVM.Status}'. ";
                ticketToUpdate.Status = newTicketVM.Status;
            }

            // If the ticket's name changes, the project also needs to update
            if (ticketToUpdate.Name != newTicketVM.Name)
            {
                log += $"Name changed from '{ticketToUpdate.Name}' to '{newTicketVM.Name}'. ";
                ticketToUpdate.Name = newTicketVM.Name;

                // Get the project to update
                var projectToUpdate = _projectRepo.GetByName(ticketToUpdate.ProjectName);
                if (projectToUpdate == null)
                {
                    _logger.LogError($"\tThe Ticket's project '{newTicketVM.ProjectName}' could not be found in the DB");
                    return false;
                }

                // Upsert the ticket to the project
                UpsertTicketBaseToProject(projectToUpdate, ticketToUpdate.Id, ticketToUpdate.Name);

                // Update the changed project
                _projectRepo.Update(projectToUpdate).Wait();
            }

            // Ensure there is a log string
            if (log.Length < 1) log = "Ticket Updated.";

            ticketToUpdate.EventLog.Add(new Log
            {
                DateAndTime = DateTime.Now,
                Event = log
            });

            _ticketRepo.Update(ticketToUpdate).Wait();

            return true;
        }

        private void UpsertTicketBaseToProject(Project projectToUpdate, string ticketId, string ticketName)
        {
            // Get the specific ticketBase item to update if it exists
            var ticketBaseToUpdate = projectToUpdate.Tickets.FirstOrDefault(t => t.Id == ticketId);

            // If it doesn't exist, insert the ticketBase to the project
            if (ticketBaseToUpdate == null)
            {
                projectToUpdate.Tickets.Add(
                     new TicketBase()
                     {
                         Id = ticketId,
                         Name = ticketName
                     });
            }
            else // otherwise update the properties
            {
                ticketBaseToUpdate.Name = ticketName;
            }
        }

        public async Task DeleteTicket(string ticketId)
        {
            // Get the Ticket to access and delete it's project and any videos
            var ticketToDelete = _ticketRepo.GetById(ticketId);
            if (ticketToDelete == null)
            {
                _logger.LogError($"\tNo ticket with id {ticketId} was found to be deleted");
                return;
            }

            // Remove the reference to the ticket in project doc
            var projectToUpdate = _projectRepo.GetByName(ticketToDelete.ProjectName);
            if (projectToUpdate != null)
            {
                var ticketBaseToDelete = projectToUpdate.Tickets.FirstOrDefault(t => t.Id == ticketId);
                projectToUpdate.Tickets.Remove(ticketBaseToDelete);

                await _projectRepo.Update(projectToUpdate);
            }
            else
            {
                _logger.LogError($"\tThe Ticket's project '{ticketToDelete.ProjectName}' could not be found in the DB");
            }

            //Delete the videos attached to the ticket
            if (ticketToDelete.Videos.Count > 0)
            {
                foreach (var video in ticketToDelete.Videos)
                {
                    var fileLocation = video.FileLocation;
                    if (fileLocation.EndsWith('/'))
                    {
                        fileLocation = fileLocation.Remove(fileLocation.Length - 1);
                    }

                    var fileName = fileLocation.Split('/').Last();
                    _videoManager.Delete(fileName);
                }
            }

            // Delete the ticket
            await _ticketRepo.Delete(ticketId);
        }
    }
}
