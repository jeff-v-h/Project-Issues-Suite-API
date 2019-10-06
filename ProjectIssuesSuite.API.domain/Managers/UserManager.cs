using ProjectIssuesSuite.API.data.Models;
using ProjectIssuesSuite.API.data.Repositories;
using ProjectIssuesSuite.API.domain.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectIssuesSuite.API.domain.Managers
{
    public class UserManager : IUserManager
    {
        private IUserRepository _userRepo;
        private readonly ILogger<UserManager> _logger;

        public UserManager(IUserRepository userRepository, ILogger<UserManager> logger)
        {
            _userRepo = userRepository;
            _logger = logger;
        }

        public ICollection<UserViewModel> GetAllUsers()
        {
            var users = _userRepo.GetAll().ToList();

            if (users.Count() < 1)
            {
                _logger.LogInformation("\tNo users were were returned from DB.");
            }

            var userVMs = new List<UserViewModel>();

            foreach (User user in users)
            {
                userVMs.Add(new UserViewModel(user));
            }

            return userVMs;
        }

        public UserViewModel GetUser(string signinName)
        {
            var user = _userRepo.GetBySigninName(signinName);

            if (user == null)
            {
                return null;
            }

            return new UserViewModel(user);
        }

        public async Task<UserViewModel> CreateUser(UserViewModel newUser)
        {
            // Ensure the User name is unique
            if (_userRepo.GetBySigninName(newUser.SigninName) != null)
            {
                _logger.LogError($"\tUser with signin name '{newUser.SigninName}' already exists in the DB");
                return null;
            }

            // Pass in the property values into a new User and add it into the Db via the repo.
            var user = new User
            {
                SigninName = newUser.SigninName,
                DisplayName = newUser.DisplayName,
                FavProjects = newUser.FavProjects,
                FavTickets = newUser.FavTickets,
                Role = newUser.Role,
                Theme = newUser.Theme
            };

            // create the User in the db and pass the new id into the view model
            await _userRepo.Create(user);
            newUser.Id = user.Id;

            // A ViewModel needs to be returned for Controller's CreatedAtRoute return method
            return newUser;
        }

        public bool ReplaceUser(string signinName, UserViewModel newUserObject)
        {
            // check to see if the there is a User with this name
            var userToUpdate = _userRepo.GetBySigninName(signinName);

            if (userToUpdate == null)
            {
                _logger.LogError($"\tUser with signin name {signinName} was not found in the DB. Nothing was updated.");
                return false;
            }

            // Partial update is not supported in CosmosDB at this current moment
            // To allow for null values, the object's properties not intending to be changed
            // should still pass in the original value (whether in front end or via earlier before here)
            userToUpdate.SigninName = newUserObject.SigninName;
            userToUpdate.DisplayName = newUserObject.DisplayName;
            userToUpdate.FavProjects = newUserObject.FavProjects;
            userToUpdate.FavTickets = newUserObject.FavTickets;
            userToUpdate.Role = newUserObject.Role;
            userToUpdate.Theme = newUserObject.Theme;

            _userRepo.Update(userToUpdate).Wait();

            return true;
        }

        public bool DeleteUser(string signinName)
        {
            var userToDelete = _userRepo.GetBySigninName(signinName);

            if (userToDelete == null)
            {
                _logger.LogError($"\tNo User with signin name '{signinName}' was found to be deleted");
                return false;
            }

            _userRepo.Delete(userToDelete.Id).Wait();

            return true;
        }
    }
}
