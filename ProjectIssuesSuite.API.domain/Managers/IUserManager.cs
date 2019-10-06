using ProjectIssuesSuite.API.domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectIssuesSuite.API.domain.Managers
{
    public interface IUserManager
    {
        ICollection<UserViewModel> GetAllUsers();
        UserViewModel GetUser(string signinName);
        Task<UserViewModel> CreateUser(UserViewModel newUser);
        bool ReplaceUser(string signinName, UserViewModel newUserObject);
        bool DeleteUser(string signinName);
    }
}
