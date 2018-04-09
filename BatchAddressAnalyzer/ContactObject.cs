using System;

namespace BatchAddressAnalyzer
{
    public class ContactObject
    {
        #region Fields
        public string AccountId { get; set; }
        public string HashedAddress { get; set; }
        public DateTime ModifiedOn { get; set; }
        public uint Count { get; set; }
        #endregion // Fields

        #region ContactObject
        public ContactObject(string accountId, string hashedAddress, DateTime modifiedOn)
        {
            AccountId = accountId;
            HashedAddress = hashedAddress;
            ModifiedOn = modifiedOn;
            Count = 1;
        }
        #endregion // ContactObject
    }
}
