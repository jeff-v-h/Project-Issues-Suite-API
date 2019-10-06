using ProjectIssuesSuite.API.data.Models;

namespace ProjectIssuesSuite.API.data.Repositories
{
    public interface IUserRepository : IDocumentRepository<User>
    {
        User GetBySigninName(string signinName);

        User GetById(string id);
    }
}
