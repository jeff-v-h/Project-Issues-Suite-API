using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjectIssuesSuite.API.data.Models
{
    public class Ticket : TicketBase
    {
        [MaxLength(500)]
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "This ticket needs to belong to a project.")]
        [JsonProperty(PropertyName = "projectName")]
        public string ProjectName { get; set; }

        [Required(ErrorMessage = "This ticket needs to have a status.")]
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "creator")]
        public string Creator { get; set; }

        [JsonProperty(PropertyName = "eventLog")]
        public ICollection<Log> EventLog { get; set; }
            = new List<Log>();

        [JsonProperty(PropertyName = "videos")]
        public ICollection<Video> Videos { get; set; }
            = new List<Video>();
    }
}
