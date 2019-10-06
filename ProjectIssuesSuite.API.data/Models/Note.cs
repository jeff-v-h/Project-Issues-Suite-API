using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectIssuesSuite.API.data.Models
{
    public class Note
    {
        [Required(ErrorMessage = "Time needed for note")]
        [JsonProperty(PropertyName = "time")]
        public float Time { get; set; }

        [Required(ErrorMessage = "Text required for the note")]
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }
    }
}
