using System;
using System.Linq;
using Microsoft.Xrm.Sdk;

namespace AddressAnalyzer
{
    class ContactPartySource : ContactSource, IContactSource
    {
        private ServiceContext _ctx;
        private IQueryable BuildContactQuery(bool isDebugMode)
        {
            return from contact in _ctx.CreateQuery("contact")
                   join party in _ctx.CreateQuery("account")
                     on contact["parentcustomerid"] equals party["accountid"]
                   where contact["crm_canal"].Equals(CANAL_COMMUNICATION_COURRIER)
                   && contact["parentcustomerid"] != null
                   && contact["fullname"] != null
                   && contact["modifiedon"] != null
                   select new
                   {
                       contactHash = new ContactMapper(this,
                           ((EntityReference)contact["parentcustomerid"]).Id.ToString(),
                           contact["fullname"].ToString(),
                           ((DateTime)contact["modifiedon"]),
                           isDebugMode)
                   };
        }

        private IQueryable BuildContactQueryWithGuid(string partyGuid, bool isDebugMode)
        {
            return from contact in _ctx.CreateQuery("contact")
                   join party in _ctx.CreateQuery("account")
                     on contact["parentcustomerid"] equals party["accountid"]
                   where contact["crm_canal"].Equals(CANAL_COMMUNICATION_COURRIER)
                   && contact["parentcustomerid"] != null
                   && contact["fullname"] != null
                   && contact["modifiedon"] != null
                   && contact["parentcustomerid"].Equals(partyGuid)
                   select new
                   {
                       contactHash = new ContactMapper(this,
                           ((EntityReference)contact["parentcustomerid"]).Id.ToString(),
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
