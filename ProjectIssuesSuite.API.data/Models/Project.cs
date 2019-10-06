using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjectIssuesSuite.API.data.Models
{
    public class Project : DocumentBase
    {
        [Required(ErrorMessage = "This project needs a name.")]
        [MaxLength(70)]
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "tickets")]
        public ICollection<TicketBase> Tickets { get; set; }
            = new List<TicketBase>();

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
