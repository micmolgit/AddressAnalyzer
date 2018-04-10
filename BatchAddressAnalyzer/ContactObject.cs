using System;
using System.Collections.Generic;

namespace BatchAddressAnalyzer
{
    public class ContactObject
    {
        #region Fields
        public string AccountId { get; set; }
        //public string HashedAddress { get; set; }
        public DateTime ModifiedOn { get; set; }
        private List<string> KnownAddresses { get; set; }
        #endregion // Fields

        #region ContactObject
        public ContactObject(string accountId, string hashedAddress, DateTime modifiedOn)
        {
            AccountId = accountId;
            ModifiedOn = modifiedOn;
            KnownAddresses = new List<string>
            {
                hashedAddress
            };
        }

        public bool DoesKnowAddress(string addressHash) => KnownAddresses.Contains(addressHash);

        public int GetKnownAddressesCount()
        {
            return KnownAddresses.Count;
        }

        public void AddToKnownAddress(string addressHash)
        {
            KnownAddresses.Add(addressHash);
        }

        public string GetFisrtKnownAddress()
        {
            return KnownAddresses[0];
        }

        public void DumpKnownAddresses(string sourceType)
        {
            Console.WriteLine("==================");
            Console.WriteLine($"account {AccountId} from {sourceType} addresses :");
            foreach (var address in KnownAddresses)
            {
                Console.WriteLine($"-----> {address}");
            }
            Console.WriteLine("==================");
        }
        #endregion // ContactObject
    }
}
