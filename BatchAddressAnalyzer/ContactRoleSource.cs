using System;
using System.Linq;

namespace BatchAddressAnalyzer
{
    class ContactRoleSource: ContactSource, IContactSource
    {
        private ServiceContext _ctx;
        private IQueryable BuildContactQuery(bool isDebugMode)
        {
            try
            {
                var query = from contact in _ctx.CreateQuery("contact")
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
                                contactHash = new ContactMapper(
                                    this,
                                    ((Guid)party["accountid"]).ToString(),
                                     (string)contact["fullname"],
                                    ((DateTime)contact["modifiedon"]),
                                    isDebugMode)
                            };
                return query;
            }
            catch (Exception Ex)
            {
                var Innermessage = Ex.InnerException != null ? Ex.InnerException.Message : "";
                Console.WriteLine($"[BuildContactQuery] {Ex.Message}  {Innermessage}");
                return null;
            }
        }

        private IQueryable BuildContactQueryWithGuid(string partyGuid, bool isDebugMode)
        {
            try
            {
                var query = from contact in _ctx.CreateQuery("contact")
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
                                contactHash = new ContactMapper(
                                    this,
                                    ((Guid)party["accountid"]).ToString(),
                                    (string)contact["fullname"],
                                    ((DateTime)contact["modifiedon"]),
                                    isDebugMode)
                            };
                return query;
            }
            catch (Exception Ex)
            {
                var Innermessage = Ex.InnerException != null ? Ex.InnerException.Message : "";
                Console.WriteLine($"[BuildContactQueryWithGuid] {Ex.Message}  {Innermessage}");
                return null;
            }
        }

        public IQueryable GetContactsQuery(ServiceContext ctx, string partyGuid = null)
        {
            _ctx = ctx;
            return partyGuid == null ? BuildContactQuery(false) : BuildContactQueryWithGuid(partyGuid, true);
        }
    }
}
