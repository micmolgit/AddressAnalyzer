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
        IQueryable GetContactsQuery(ServiceContext ctx, string partyGuid = null);
        Dictionary<string, ContactObject> GetDictionary();
        void UpdateDictionary(string accountId, string hashedAddress, DateTime modifiedOn);
    }
}
