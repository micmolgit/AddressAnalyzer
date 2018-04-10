using System;
using System.Linq;

namespace BatchAddressAnalyzer
{
    class ContactRoleSource: ContactSource, IContactSource
    {
        #region Fields
        private ServiceContext _ctx;
        #endregion // Fields

        #region BuildContactQuery
        private IQueryable BuildContactQuery()
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
                                    (ContactRoleSource)this,
                                    ((Guid)party["accountid"]).ToString(),
                                     (string)contact["fullname"],
                                    ((DateTime)contact["modifiedon"]))
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
        #endregion BuildContactQuery

        #region BuildContactQueryWithGuid
        private IQueryable BuildContactQueryWithGuid(string partyGuid)
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
                                    (ContactRoleSource)this,
                                    ((Guid)party["accountid"]).ToString(),
                                    (string)contact["fullname"],
                                    ((DateTime)contact["modifiedon"]))
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
        #endregion // BuildContactQueryWithGuid

        #region GetContactsQuery
        public IQueryable GetContactsQuery(ServiceContext ctx, string partyGuid = null)
        {
            _ctx = ctx;
            return partyGuid == null ? BuildContactQuery() : BuildContactQueryWithGuid(partyGuid);
        }
        #endregion // GetContactsQuery
    }
}
