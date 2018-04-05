using System;
using System.Text;
using System.Security.Cryptography;

namespace AddressAnalyzer
{
    public class ContactMapper
    {
        #region Properties
        private ContactObject _contactObject { get; set; }
        private static readonly MD5 _md5Hash = MD5.Create();
        private IContactSource _contactSrc;
        #endregion Properties

        #region ContactMapper
        public ContactMapper(IContactSource contactSrc, string accountId, string inputAddress, DateTime modifiedOn, bool isDebugMode)
        {
            _contactSrc = contactSrc;
            var trimmedAddress = inputAddress.ToString().Replace(" ", "").ToLower();
            var hashedAddress = BuildHashMap(accountId, trimmedAddress, modifiedOn);

            // DEBUG : QCOREKA-2008
            if (isDebugMode)
            {
                Console.WriteLine($"===\nSource type : {contactSrc.GetType()}:\n" +
                    $"inputAddress : [{inputAddress}]\n" +
                    $"modifiedOn : {modifiedOn}");
            }
            // DEBUG : QCOREKA-2008
        }
        #endregion // ContactMapper

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

        #region BuildHashMap
        private string BuildHashMap(string accountId, string inputAddress, DateTime modifiedOn)
        {
            string hashedAddress = null;
            try
            {
                // computing an hash from the address provided
                hashedAddress = GetMd5Hash(inputAddress);
                if (!_contactSrc.GetDictionary().ContainsKey(accountId))
                {
                    // the accountId is not already known, we're adding a new contact entry to the dictionary
                    _contactObject = new ContactObject(accountId, hashedAddress, modifiedOn);
                    _contactSrc.GetDictionary().Add(accountId, _contactObject);
                }
                else
                {
                    // the accountId is known, we're just updating it
                    _contactSrc.UpdateDictionary(accountId, hashedAddress, modifiedOn);
                }

            }
            catch (Exception ex)
            {
                var innerExMsg = ex.InnerException != null ? ex.InnerException.Message : "";
                Console.WriteLine($"BuildHashMap() : Add/Modify entry to Hashmap error {ex.Message} : {innerExMsg}");
            }

            return hashedAddress;
        }

        #endregion // BuildHashMap
    }
}
