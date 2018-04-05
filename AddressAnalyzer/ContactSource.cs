using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AddressAnalyzer
{
    abstract class ContactSource
    {
        public static readonly int CLIENT_FACTURATION_ROLE = 100000000;
        public static readonly int CANAL_COMMUNICATION_COURRIER = 100000004;
        Dictionary<string, ContactObject> ContactDictionary = new Dictionary<string, ContactObject>();
        public Dictionary<string, ContactObject> GetDictionary()
        {
            return ContactDictionary;
        }
        public void UpdateDictionary(string accountId, string hashedAddress, DateTime modifiedOn)
        {
            var contactEntry = ContactDictionary[accountId];
            if (contactEntry != null)
            {
                contactEntry.HashedAddress = hashedAddress;
                contactEntry.ModifiedOn = modifiedOn;
            }
        }

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
                    Console.WriteLine($"Executing data collection query : Contacts from type 'Courrier' related to {sourceName} ...");
                    foreach (var contact in ContactsQuery) { cptContact++; };
                    sw.Stop();
                }
                catch (Exception Ex)
                {
                    var Innermessage = Ex.InnerException != null ? Ex.InnerException.Message : "";
                    Console.WriteLine($"[GetContactCountFromParty] {Ex.Message}  {Innermessage}");
                }
            });

            return cptContact;
        }
    }
}
