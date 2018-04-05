using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddressAnalyzer
{
    public class ContactObject
    {
        public string AccountId { get; set; }
        public string HashedAddress { get; set; }
        public DateTime ModifiedOn { get; set; }

        public ContactObject(string accountId, string hashedAddress, DateTime modifiedOn)
        {
            AccountId = accountId;
            HashedAddress = hashedAddress;
            ModifiedOn = modifiedOn;
        }
    }
}
