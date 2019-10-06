using ProjectIssuesSuite.API.data.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjectIssuesSuite.API.domain.Models
{
    public class TicketViewModel
    {
        public TicketViewModel(Ticket ticket)
        {
            Id = ticket.Id;
            Name = ticket.Name;
            Description = ticket.Description;
            ProjectName = ticket.ProjectName;
            Status = ticket.Status;
            Videos = ticket.Videos;
            EventLog = ticket.EventLog;
            Creator = ticket.Creator;
        }

        public TicketViewModel() { }

        public string Id { get; set; }

        [Required(ErrorMessage = "You should provide a name for this ticket.")]
        [MaxLength(70)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required(ErrorMessage = "This ticket needs to belong to a project.")]
        [MaxLength(70)]
        public string ProjectName { get; set; }

        public string Status { get; set; }

        public string Creator { get; set; }

        public ICollection<Log> EventLog { get; set; }
            = new List<Log>();

        public ICollection<Video> Videos { get; set; }
            = new List<Video>();

        public IFormFileCollection VideoFiles { get; set; }

        public ICollection<string> VideoThumbnails { get; set; }
            = new List<string>();
    }
}
