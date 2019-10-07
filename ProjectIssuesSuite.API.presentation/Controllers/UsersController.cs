using ProjectIssuesSuite.API.domain.Managers;
using ProjectIssuesSuite.API.domain.Models;
using LazyCache;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectIssuesSuite.API.presentation.Controllers
{
    [Route("api/users")]
    public class UsersController : Controller
    {
        private IUserManager _manager { get; set; }
        private readonly IAppCache _cache;
        private TimeSpan _cacheExpiry { get; set; } = new TimeSpan(0, 0, 1);

        public UsersController(IUserManager manager, IAppCache cache)
        {
            _manager = manager;
            _cache = cache;
        }

        [HttpGet("")]
        public IActionResult GetAllUsers()
        {
            Func<ICollection<UserViewModel>> usersGetter = () => _manager.GetAllUsers();

            var usersCached = _cache.GetOrAdd("UsersController.GetUsers", usersGetter, _cacheExpiry);

            return Ok(usersCached);
        }

        [HttpGet("{signinName}", Name = "GetUser")]
        public IActionResult GetUser(string signinName)
        {
            Func<UserViewModel> userGetter = () => _manager.GetUser(signinName);

            UserViewModel userCached = _cache.GetOrAdd(
                "UsersController.GetUser." + signinName,
                userGetter,
                _cacheExpiry);

            if (userCached == null)
            {
                return NotFound($"User with signin name '{signinName}' was not found.");
            }

            return Ok(userCached);
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateUser([FromBody] UserViewModel user)
        {
            if (user == null)
            {
                return BadRequest("Please provide details to create a user.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // null is returned if the User was not created successfully
            UserViewModel userVM = await _manager.CreateUser(user);
            if (userVM == null)
            {
                return BadRequest($"The User name '{user.SigninName}' already exists.");
            }

            // User is successfully created. Return the uri to the created User.
            return CreatedAtRoute("GetUser", new { signinName = userVM.SigninName }, userVM);
        }

        [HttpPost("{signinName}")]
        public IActionResult UpdateUser(string signinName, [FromBody] UserViewModel newUserObject)
        {
            if (newUserObject == null)
            {
                return BadRequest("Please provide details to update a User.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // replace the User with the new info above
            bool userIsUpdated = _manager.ReplaceUser(signinName, newUserObject);

            if (!userIsUpdated)
            {
                return NotFound($"User with signin name '{signinName}' was not found. No update was executed.");
            }

            return NoContent();
        }

        [HttpDelete("{signinName}")]
        public IActionResult DeleteUser(string signinName)
        {
            bool userIsDeleted = _manager.DeleteUser(signinName);
            if (!userIsDeleted)
            {
                return NotFound($"User with signin name '{signinName}' was not found. No delete was executed.");
            }

            return NoContent();
        }
    }
}
