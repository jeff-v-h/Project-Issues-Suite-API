using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjectIssuesSuite.API.data.Models
{
    public class Video : DocumentBase
    {
        [Required(ErrorMessage = "This video needs a title.")]
        [MaxLength(70)]
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "fileLocation")]
        public string FileLocation { get; set; }

        [JsonProperty(PropertyName = "thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty(PropertyName = "notes")]
        public ICollection<Note> Notes { get; set; }
            = new List<Note>();
    }
}
