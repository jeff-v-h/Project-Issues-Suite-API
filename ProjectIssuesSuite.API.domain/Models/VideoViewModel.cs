using Microsoft.AspNetCore.Http;

namespace ProjectIssuesSuite.API.domain.Models
{
    public class VideoViewModel
    {
        public IFormFileCollection VideoFiles { get; set; }
    }
}
