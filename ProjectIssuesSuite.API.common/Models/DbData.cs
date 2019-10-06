using System.Collections.Generic;

namespace ProjectIssuesSuite.API.common.Models
{
    public class DbData
    {
        public DbData()
        {
            // default data
            DbName = "DatabaseName";
        }

        public string DbName { get; set; }
        // Collections
        public string ProjectsCollectionName { get; set; }
        public string TicketsCollectionName { get; set; }
        public string UsersCollectionName { get; set; }
        // ATP
        public string AtpId { get; set; }
        public string AtpName { get; set; }
        // ATP Ticket 1
        public string AtpTicketId1 { get; set; }
        public string AtpTicketName1 { get; set; }
        public string AtpTicketDesc1 { get; set; }
        // ATP Ticket 1 Vid 1
        public string AtpVidTitle1 { get; set; }

        // ATP Ticket 1 Vid 2
        public string AtpVidTitle2 { get; set; }


        // ATP Ticket 2 
        public string AtpTicketId2 { get; set; }
        public string AtpTicketName2 { get; set; }
        public string AtpTicketDesc2 { get; set; }
        // ATP Ticket 2 Vid 
        public string AtpVidTitle3 { get; set; }


        // BT
        public string BtId { get; set; }
        public string BtName { get; set; }

        // BT Ticket 2 
        public string BtTicketId1 { get; set; }
        public string BtTicketName1 { get; set; }
        public string BtTicketDesc1 { get; set; }
        // BT Ticket 2 Vid 
        public string BtVidTitle1 { get; set; }


        // Users
        public string User1 { get; set; }
        public string User2 { get; set; }
        public string User3 { get; set; }
    }
}
