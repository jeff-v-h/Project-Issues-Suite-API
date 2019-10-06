using System.Linq;
using System.Threading.Tasks;

namespace ProjectIssuesSuite.API.data.Repositories
{
    public interface IDocumentRepository<T>
    {
        Task<T> Create(T document);
        IQueryable<T> GetAll();
        Task Update(T updatedDoc);
        Task Delete(string id);
    }
}
