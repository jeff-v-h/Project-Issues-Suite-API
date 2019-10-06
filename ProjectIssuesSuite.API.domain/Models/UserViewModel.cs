using ProjectIssuesSuite.API.data.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjectIssuesSuite.API.domain.Models
{
    public class UserViewModel
    {
        public UserViewModel(User user)
        {
            Id = user.Id;
            SigninName = user.SigninName;
            DisplayName = user.DisplayName;
            FavProjects = user.FavProjects;
            FavTickets = user.FavTickets;
            Role = user.Role;
            Theme = user.Theme;
        }

        public UserViewModel() { }

        public string Id { get; set; }

        [Required(ErrorMessage = "This user needs an email signin name.")]
        [MaxLength(254)]
        public string SigninName { get; set; }

        [Required(ErrorMessage = "This user needs a display name.")]
        [MaxLength(70)]
        public string DisplayName { get; set; }

        public ICollection<ProjectBase> FavProjects { get; set; }
            = new List<ProjectBase>();
        public ICollection<TicketBase> FavTickets { get; set; }
            = new List<TicketBase>();

        [MaxLength(70)]
        public string Role { get; set; }
        public string Theme { get; set; }
    }
}
