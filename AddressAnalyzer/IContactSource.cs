using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AddressAnalyzer
{
    public interface IContactSource
    {
        IQueryable GetContactsQuery(ServiceContext ctx);
        Dictionary<string, ContactObject> GetDictionary();
        void UpdateDictionary(string accountId, string Address, DateTime modifiedOn);
    }
}
