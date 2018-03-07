using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddressAnalyzer
{
    class ContactRoleSource: ContactSource, IContactSource
    {
        public IQueryable GetContactsQuery(ServiceContext ctx)
        {
            return
                from contact in ctx.CreateQuery("contact")
                join role_client in ctx.CreateQuery("crm_roleclient")
                   on contact["crm_role_client_id"] equals role_client["crm_roleclientid"]
                join party in ctx.CreateQuery("account")
                  on role_client["crm_account_id"] equals party["accountid"]
                join client_facturation in ctx.CreateQuery("crm_clientdefacturation")
                   on role_client["crm_clientdefacturation_id"] equals client_facturation["crm_clientdefacturationid"]
                where role_client["crm_role"].Equals(CLIENT_FACTURATION_ROLE)
                && contact["crm_canal"].Equals(CRM_CANAL_COMMUNICATION)
                && contact["fullname"] != null
                && party["accountid"] != null
                && contact["modifiedon"] != null
                select new
                {
                    contactHash = new ContactMapper((IContactSource)this,
                        ((Guid)party["accountid"]).ToString(),
                        contact["fullname"].ToString(),
                        ((DateTime)contact["modifiedon"]))
                };
        }

      
    }
}
