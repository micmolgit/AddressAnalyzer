using System;
using System.Text;
using System.Security.Cryptography;

namespace BatchAddressAnalyzer
{
    public class ContactMapper
    {
        #region Properties
        private static readonly MD5 _md5Hash = MD5.Create();
        private IContactSource _contactSrc;
        #endregion Properties

        #region ContactMapper
        public ContactMapper(IContactSource contactSrc, string accountId, string inputAddress, DateTime modifiedOn)
        {
            _contactSrc = contactSrc;
            AddAddressToDictionary(contactSrc, accountId, inputAddress, modifiedOn);
        }
        #endregion // ContactMapper

        #region AddAddressToDictionary
        private void AddAddressToDictionary(IContactSource contactSrc, string accountId, string inputAddress, DateTime modifiedOn)
        {
            var trimmedAddress = inputAddress.ToString().Replace(" ", "").ToLower();
            var contactEntry = ProcessAddressToHashmap(accountId, trimmedAddress, modifiedOn);

            if (BatchAddressAnalyzer.IsDebugMode)
            {
                BatchAddressAnalyzer.DebugMessage(String.Format($"[{contactSrc.GetType()}]\nprocessing address ({contactEntry.GetFisrtKnownAddress()}):\n[{inputAddress}]"));
            }
        }
        #endregion // AddAddressToDictionary

        #region GetMd5Hash
        public static string GetMd5Hash(string input)
        {
            if (string.IsNullOrEmpty(input))
                return " ";

            // Convert the input string to a byte array and compute the hash.
            byte[] data = _md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
        #endregion //GetMd5Hash

        #region ProcessAddressToHashmap
        private ContactObject ProcessAddressToHashmap(string accountId, string inputAddress, DateTime modifiedOn)
        {
            ContactObject contactEntry = null;
            try
            {
                // computing an hash from the address provided
                var hashedAddress = GetMd5Hash(inputAddress);

                // getting a reference to Party or Role dictionary
                var currentDictionary = _contactSrc.GetDictionary();

                if (!currentDictionary.ContainsKey(accountId))
                {
                    // the accountId is not already known, we're adding a new contact entry to the dictionary
                    contactEntry = new ContactObject(accountId, hashedAddress, modifiedOn);
                    currentDictionary.Add(accountId, contactEntry);
                }
                else
                {
                    // the accountId is known, we're just updating it
                    contactEntry = _contactSrc.UpdateDictionary(accountId, hashedAddress, modifiedOn);
                }

            }
            catch (Exception ex)
            {
                var innerExMsg = ex.InnerException != null ? ex.InnerException.Message : "";
                Console.WriteLine($"ProcessAddressToHashmap() : Add/Modify entry to Hashmap error {ex.Message} : {innerExMsg}");
            }

            return contactEntry;
        }

        #endregion // ProcessAddressToHashmap
    }
}
