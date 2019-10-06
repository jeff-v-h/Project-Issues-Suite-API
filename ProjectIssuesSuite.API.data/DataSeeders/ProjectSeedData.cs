using ProjectIssuesSuite.API.common.Models;
using ProjectIssuesSuite.API.data.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectIssuesSuite.API.data.DataSeeders
{
    public class ProjectSeedData
    {
        private List<Project> _projectsList { get; set; }
        private List<Ticket> _ticketsList { get; set; }
        private DbSettings _dbSettings;
        private DocumentClient _client { get; set; }
        private DbData _dbData;
        private NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private string _dbName { get; set; }
        private string _projectsCollectionName { get; set; }
        private string _ticketsCollectionName { get; set; }
        private string _usersCollectionName { get; set; }

        public ProjectSeedData(IOptions<DbSettings> dbSettings, IOptions<DbData> dbData)
        {
            _dbSettings = dbSettings.Value;
            _client = new DocumentClient(new Uri(_dbSettings.EndpointUri), _dbSettings.PrimaryKey);
            _dbData = dbData.Value;

            _dbName = _dbData.DbName;
            _projectsCollectionName = _dbData.ProjectsCollectionName;
            _ticketsCollectionName = _dbData.TicketsCollectionName;
            _usersCollectionName = _dbData.UsersCollectionName;

            // init seed data lists
            _projectsList = new List<Project>()
            {
                new Project()
                {
                    Id = _dbData.AtpId,
                    Name = _dbData.AtpName,
                    Tickets = new List<TicketBase>()
                    {
                        new TicketBase()
                        {
                            Id = _dbData.AtpTicketId1,
                            Name = _dbData.AtpTicketName1
                        },
                        new TicketBase()
                        {
                            Id = _dbData.AtpTicketId2,
                            Name = _dbData.AtpTicketName2
                        }
                    }

                },
                new Project()
                {
                    Id = _dbData.BtId,
                    Name = _dbData.BtName,
                    Tickets = new List<TicketBase>()
                    {
                        new TicketBase()
                        {
                            Id = _dbData.BtTicketId1,
                            Name = _dbData.BtTicketName1
                        }
                    }

                }
            };

            _ticketsList = new List<Ticket>()
            {
                new Ticket()
                {
                    Id = _dbData.AtpTicketId1,
                    Name = _dbData.AtpTicketName1,
                    Description = _dbData.AtpTicketDesc1,
                    ProjectName = _dbData.AtpName
                },
                new Ticket()
                {
                    Id = _dbData.AtpTicketId2,
                    Name = _dbData.AtpTicketName2,
                    Description = _dbData.AtpTicketDesc2,
                    ProjectName = _dbData.AtpName
                },
                new Ticket()
                {
                    Id = _dbData.BtTicketId1,
                    Name = _dbData.BtTicketName1,
                    Description = _dbData.BtTicketDesc1,
                    ProjectName = _dbData.BtName
                },
            };
        }

        public async Task InitDbAndCollections()
        {
            // Create a database if it does not exist
            await _client.CreateDatabaseIfNotExistsAsync(new Database { Id = _dbName });

            // create collections if they do not exist
            var dbUri = UriFactory.CreateDatabaseUri(_dbName);
            await _client.CreateDocumentCollectionIfNotExistsAsync(dbUri, new DocumentCollection { Id = _projectsCollectionName });
            await _client.CreateDocumentCollectionIfNotExistsAsync(dbUri, new DocumentCollection { Id = _ticketsCollectionName });
            await _client.CreateDocumentCollectionIfNotExistsAsync(dbUri, new DocumentCollection { Id = _usersCollectionName });
        }

        public async Task InitDocuments()
        {
            // Check for documents. If none exist, add data to these databases
            var docFeed = await _client.ReadDocumentFeedAsync(UriFactory.CreateDocumentCollectionUri(_dbName, _projectsCollectionName));
            if (docFeed.Any())
            {
                _logger.Info("\tA document exists in the project collection, so no additional document was added to the database.");
            }
            else
            {
                // get the projects from the seed data file and create documents of each in the database
                Project atpProject = _projectsList.FirstOrDefault(p => p.Name == _dbData.AtpName);
                await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_dbName, _projectsCollectionName), atpProject);
                Project btProject = _projectsList.FirstOrDefault(p => p.Name == _dbData.BtName);
                await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_dbName, _projectsCollectionName), btProject);

                Ticket atpTicket1 = _ticketsList.FirstOrDefault(p => p.Name == _dbData.AtpTicketName1);
                await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_dbName, _ticketsCollectionName), atpTicket1);
                Ticket atpTicket2 = _ticketsList.FirstOrDefault(p => p.Name == _dbData.AtpTicketName2);
                await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_dbName, _ticketsCollectionName), atpTicket2);
                Ticket btTicket1 = _ticketsList.FirstOrDefault(p => p.Name == _dbData.BtTicketName1);
                await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_dbName, _ticketsCollectionName), btTicket1);

                _logger.Info("\tProject and Ticket document seed data added to the database.");
            }
        }
    }
}
