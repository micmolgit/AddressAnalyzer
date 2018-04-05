using System;
using System.Linq;

namespace AddressAnalyzer
{
    class ContactRoleSource: ContactSource, IContactSource
    {
        private ServiceContext _ctx;
        private IQueryable BuildContactQuery(bool isDebugMode)
        {
            return from contact in _ctx.CreateQuery("contact")
                   join role_client in _ctx.CreateQuery("crm_roleclient")
                      on contact["crm_role_client_id"] equals role_client["crm_roleclientid"]
                   join party in _ctx.CreateQuery("account")
                     on role_client["crm_account_id"] equals party["accountid"]
                   join client_facturation in _ctx.CreateQuery("crm_clientdefacturation")
                      on role_client["crm_clientdefacturation_id"] equals client_facturation["crm_clientdefacturationid"]
                   where role_client["crm_role"].Equals(CLIENT_FACTURATION_ROLE)
                   && contact["crm_canal"].Equals(CANAL_COMMUNICATION_COURRIER)
                   && contact["fullname"] != null
                   && party["accountid"] != null
                   && contact["modifiedon"] != null
                   select new
                   {
                       contactHash = new ContactMapper(this,
                           ((Guid)party["accountid"]).ToString(),
                           contact["fullname"].ToString(),
                           ((DateTime)contact["modifiedon"]),
                           isDebugMode)
                   };
        }

        private IQueryable BuildContactQueryWithGuid(string partyGuid, bool isDebugMode)
        {
            return from contact in _ctx.CreateQuery("contact")
                   join role_client in _ctx.CreateQuery("crm_roleclient")
                      on contact["crm_role_client_id"] equals role_client["crm_roleclientid"]
                   join party in _ctx.CreateQuery("account")
                     on role_client["crm_account_id"] equals party["accountid"]
                   join client_facturation in _ctx.CreateQuery("crm_clientdefacturation")
                      on role_client["crm_clientdefacturation_id"] equals client_facturation["crm_clientdefacturationid"]
                   where role_client["crm_role"].Equals(CLIENT_FACTURATION_ROLE)
                   && contact["crm_canal"].Equals(CANAL_COMMUNICATION_COURRIER)
                   && contact["fullname"] != null
                   && party["accountid"] != null
                   && contact["modifiedon"] != null
                   && party["accountid"].Equals(partyGuid)
                   select new
                   {
                       contactHash = new ContactMapper(this,
                           ((Guid)party["accountid"]).ToString(),
                           contact["fullname"].ToString(),
                           ((DateTime)contact["modifiedon"]),
                           isDebugMode)
                   };
        }

        public IQueryable GetContactsQuery(ServiceContext ctx, string partyGuid = null)
        {
            _ctx = ctx;
            return partyGuid == null ? BuildContactQuery(false) : BuildContactQueryWithGuid(partyGuid, true);
        }
    }
}
