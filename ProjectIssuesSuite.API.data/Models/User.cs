using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjectIssuesSuite.API.data.Models
{
    public class User : DocumentBase
    {
        [Required(ErrorMessage = "This user needs an email signin name.")]
        [MaxLength(254)]
        [JsonProperty(PropertyName = "signinName")]
        public string SigninName { get; set; }

        [Required(ErrorMessage = "This user needs a display name.")]
        [MaxLength(70)]
        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "favProjects")]
        public ICollection<ProjectBase> FavProjects { get; set; }
            = new List<ProjectBase>();

        [JsonProperty(PropertyName = "favTickets")]
        public ICollection<TicketBase> FavTickets { get; set; }
            = new List<TicketBase>();

        [MaxLength(70)]
        [JsonProperty(PropertyName = "role")]
        public string Role { get; set; }

        [JsonProperty(PropertyName = "theme")]
        public string Theme { get; set; }
    }
}