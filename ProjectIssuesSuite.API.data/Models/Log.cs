using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectIssuesSuite.API.data.Models
{
    public class Log
    {
        [Required(ErrorMessage = "Date and time needed for log")]
        [JsonProperty(PropertyName = "dateAndTime")]
        public DateTime DateAndTime { get; set; }

        [JsonProperty(PropertyName = "event")]
        public string Event { get; set; }
    }
}
