using ProjectIssuesSuite.API.common.Models;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ProjectIssuesSuite.API.data.Repositories
{
    public class UserRepository : CosmosRepository<Models.User>, IUserRepository
    {
        public UserRepository(IOptions<DbSettings> dbSettings, ILogger<UserRepository> logger, IDocumentClient client) : base(dbSettings, logger, client)
        {
        }

        public Models.User GetBySigninName(string signinName)
        {
            return GetFirstOrDefault(x => x.SigninName.ToUpper() == signinName.ToUpper());
        }

        public Models.User GetById(string id)
        {
            return GetFirstOrDefault(x => x.Id.ToUpper() == id.ToUpper());
        }
    }
}