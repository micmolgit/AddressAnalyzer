using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace AddressAnalyzer
{
    public class ContactMapper
    {
        public ContactObject contactObject { get; set; }
        static readonly private MD5 _md5Hash = MD5.Create();
        IContactSource _contactSrc;

        private void BuildHashMap(string accountId, string inputAddress, DateTime modifiedOn)
        {
            try
            {
                // making sure the input address is not null or empty
                var address = string.IsNullOrEmpty(inputAddress) ? "#" : inputAddress;

                // computing an hash from the address provided
                var hashedAddress = GetMd5Hash(address.ToString().Replace(" ", "").ToLower());

                if (!_contactSrc.GetDictionary().ContainsKey(accountId))
                {
                    // the accountId is not alraedy known, we're adding a new contact entry to the dictionary
                    contactObject = new ContactObject(accountId, hashedAddress, modifiedOn);
                    _contactSrc.GetDictionary().Add(accountId, contactObject);
                }
                else
                {
                    // the accountId is known, we're just updating it
                    _contactSrc.UpdateDictionary(accountId, hashedAddress, modifiedOn);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("BuildHashMap() : Add/Modify entry to Hashmap error {0} : {1}",
                   ex.Message,
                ex.InnerException != null ? ex.InnerException.Message : "");
            }
        }

        public ContactMapper(IContactSource contactSrc, string accountId, string inputAddress, DateTime modifiedOn)
        {
            _contactSrc = contactSrc;
            BuildHashMap(accountId, inputAddress, modifiedOn);
        }

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
}
}
