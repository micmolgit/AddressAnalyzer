using System;
using System.Linq;

using Microsoft.Xrm.Sdk;

namespace AddressAnalyzer
{
    class ContactPartySource : ContactSource, IContactSource
    {
        public IQueryable GetContactsQuery(ServiceContext ctx) => from contact in ctx.CreateQuery("contact")
                                                                  join party in ctx.CreateQuery("account")
                                                                    on contact["parentcustomerid"] equals party["accountid"]
                                                                  where contact["crm_canal"].Equals(CRM_CANAL_COMMUNICATION)
                                                                  && contact["parentcustomerid"] != null
                                                                  && contact["fullname"] != null
                                                                  && contact["modifiedon"] != null
                                                                  select new
                                                                  {
                                                                      contactHash = new ContactMapper((IContactSource)this,
                                                                          ((EntityReference)contact["parentcustomerid"]).Id.ToString(),
                                                                          contact["fullname"].ToString(),
                                                                          ((DateTime)contact["modifiedon"]))
                                                                  };
    }
}
