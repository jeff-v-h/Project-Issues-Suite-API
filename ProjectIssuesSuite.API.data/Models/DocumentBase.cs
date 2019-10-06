using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace ProjectIssuesSuite.API.data.Models
{
    public class DocumentBase
    {
        [Key]
        // the property name will be whats in the database.
        // if propname is capital Id, the db will still autoassign it's own lowercase id.
        [JsonProperty(PropertyName = "id", Order = -3)]
        // CosmosDB id field uses string type, not int. if id not provided, it will auto assign a GUID
        public string Id { get; set; }
    }
}
