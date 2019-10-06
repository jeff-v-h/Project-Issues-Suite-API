using ProjectIssuesSuite.API.data.Models;
using ProjectIssuesSuite.API.data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectIssuesSuite.API.domain.Frameworks
{
    public class ServiceManager
    {
        public static void InjectServices(IServiceCollection services)
        {
            services.AddTransient<IProjectRepository, ProjectRepository>();
            services.AddTransient<IDocumentRepository<Project>, CosmosRepository<Project>>();

            services.AddTransient<ITicketRepository, TicketRepository>();
            services.AddTransient<IDocumentRepository<Ticket>, CosmosRepository<Ticket>>();

            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IDocumentRepository<User>, CosmosRepository<User>>();

            services.AddTransient<IVideoRepository, VideoRepository>();
        }
    }
}
