using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace ProjectIssuesSuite.API.data.Models
{
    public class ProjectBase : DocumentBase
    {
        [Required(ErrorMessage = "This project needs a name.")]
        [MaxLength(70)]
        [JsonProperty(PropertyName = "name", Order = -2)]
        public string Name { get; set; }
    }
}
