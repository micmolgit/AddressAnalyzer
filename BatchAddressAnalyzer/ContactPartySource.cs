using System;
using System.Linq;
using Microsoft.Xrm.Sdk;

namespace BatchAddressAnalyzer
{
    class ContactPartySource : ContactSource, IContactSource
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
                            join party in _ctx.CreateQuery("account")
                              on contact["parentcustomerid"] equals party["accountid"]
                            where contact["crm_canal"].Equals(CANAL_COMMUNICATION_COURRIER)
                            && contact["parentcustomerid"] != null
                            && contact["fullname"] != null
                            && contact["modifiedon"] != null
                            select new
                            {
                                contactHash = new ContactMapper(
                                    (ContactPartySource)this,
                                    ((EntityReference)contact["parentcustomerid"]).Id.ToString(),
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
        #endregion // BuildContactQuery

        #region BuildContactQueryWithGuid
        private IQueryable BuildContactQueryWithGuid(string partyGuid)
        {
            try
            {
                var query = from contact in _ctx.CreateQuery("contact")
                                   join party in _ctx.CreateQuery("account")
                                     on contact["parentcustomerid"] equals party["accountid"]
                                   where contact["crm_canal"].Equals(CANAL_COMMUNICATION_COURRIER)
                                   && contact["parentcustomerid"] != null
                                   && contact["fullname"] != null
                                   && contact["modifiedon"] != null
                                   && contact["parentcustomerid"].Equals(partyGuid)
                                   select new
                                   {
                                       contactHash = new ContactMapper(
                                           (ContactPartySource)this,
                                           ((EntityReference)contact["parentcustomerid"]).Id.ToString(),
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
