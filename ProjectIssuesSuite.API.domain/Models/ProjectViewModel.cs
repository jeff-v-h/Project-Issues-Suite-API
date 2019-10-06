using ProjectIssuesSuite.API.data.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjectIssuesSuite.API.domain.Models
{
    public class ProjectViewModel
    {
        public ProjectViewModel(Project project)
        {
            Id = project.Id;
            Name = project.Name;
            Tickets = project.Tickets;
        }

        public ProjectViewModel() { }

        public string Id { get; set; }

        [Required(ErrorMessage = "You should provide a name for this project.")]
        [MaxLength(70)]
        public string Name { get; set; }
        public ICollection<TicketBase> Tickets { get; set; }
            = new List<TicketBase>();
    }
}
