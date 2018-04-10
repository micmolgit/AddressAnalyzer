using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

namespace BatchAddressAnalyzer
{
    abstract class ContactSource
    {
        #region Fields
        public static readonly int CLIENT_FACTURATION_ROLE = 100000000;
        public static readonly int CANAL_COMMUNICATION_COURRIER = 100000004;
        Dictionary<string, ContactObject> ContactDictionary = new Dictionary<string, ContactObject>();
        public Dictionary<string, ContactObject> GetDictionary()
        {
            return ContactDictionary;
        }
        #endregion // Fields

        #region UpdateDictionary
        public ContactObject UpdateDictionary(string accountId, string hashedAddress, DateTime modifiedOn)
        {
            ContactObject contactEntry = null;
            try
            {
                if (!ContactDictionary.ContainsKey(accountId))
                {
                    Console.WriteLine($"UpdateDictionary() : {accountId} entry could not be found");
                    return null;
                }
                ContactDictionary.TryGetValue(accountId, out contactEntry);
                if (contactEntry != null)
                {
                    // The address is not matching any known address, we're adding it to the known list
                    if (!contactEntry.DoesKnowAddress(hashedAddress))
                    {
                        contactEntry.AddToKnownAddress(hashedAddress);
                        contactEntry.ModifiedOn = modifiedOn;
                    }
                }
            }
            catch (Exception Ex)
            {
                var Innermessage = Ex.InnerException != null ? Ex.InnerException.Message : "";
                Console.WriteLine($"[UpdateDictionary] {Ex.Message}  {Innermessage}");
            }

            return (contactEntry);
        }
        #endregion // UpdateDictionary

        #region GetContactsCountAsync
        public async Task<uint> GetContactsCountAsync(IContactSource contactSrc, ServiceContext ctx, string sourceName, string partyGuid = null)
        {
            uint cptContact = 0;

            await Task<uint>.Run(() =>
            {
                Stopwatch sw = new Stopwatch();
                try
                {
                    var ContactsQuery = contactSrc.GetContactsQuery(ctx, partyGuid);

                    sw.Start();
                    Console.WriteLine($"Executing data collection query :\n=> Contacts from type 'Courrier' related to {sourceName} ...\n");
                    foreach (var contact in ContactsQuery) { cptContact++; };
                    sw.Stop();
                }
                catch (Exception Ex)
                {
                    var Innermessage = Ex.InnerException != null ? Ex.InnerException.Message : "";
                    var stackTrace = Ex.StackTrace != null ? Ex.StackTrace : "";
                    Console.WriteLine($"[GetContactsCountAsync : {contactSrc.GetType()} {cptContact}] : => \n{Ex.Message}\n=> \n{Innermessage}\n=> {stackTrace}");
                }
            });

            return cptContact;
        }
        #endregion // GetContactsCountAsync
    }
}
