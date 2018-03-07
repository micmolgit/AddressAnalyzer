using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

namespace AddressAnalyzer
{
    class AddressAnalyzer
    {   
        private readonly DcrmConnector _dcrmConnector = new DcrmConnector();
        private readonly Stopwatch _swGlobal = new Stopwatch();
        private StreamWriter _streamWriter = null;
        private ContactPartySource _partyContactSrc = new ContactPartySource();
        private ContactRoleSource _roleContactSrc = new ContactRoleSource();

        private static string OUTPUT_CSV_FILENAME = "ImpactedPartiesExport.csv";
        private static string SOURCE_PARTY = "Party";
        private static string SOURCE_ROLE = "Role";
        
        public AddressAnalyzer()
        {
            _dcrmConnector = new DcrmConnector();
        }

        public void Terminate()
        {
            if (_dcrmConnector != null)
                _dcrmConnector.Disconnect();

            Console.WriteLine("Press <Enter> to exit.");
            Console.ReadLine();
        }

        async Task<bool> WriteCvsLineAsync(string outputText)
        {
            bool bSuccess = false;

            try
            {
                if (_streamWriter == null)
                {
                    Console.WriteLine("Exporting the impacted parties to : {0} ...", OUTPUT_CSV_FILENAME);
                    _streamWriter = new StreamWriter(OUTPUT_CSV_FILENAME);
                }
                await _streamWriter.WriteLineAsync(outputText);
                bSuccess = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[WriteCvsLine] {0} {1}",
                   ex.Message,
                    ex.InnerException != null ? ex.InnerException.Message : "");
            }

            return bSuccess;
        }

        public async Task<uint> ProcessImpactedAccounts()
        {
            uint impactedEntitiesCpt = 0;

            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                // Creating Export file with CVS Header [GUID | MODIFIED ON]
                var outputText = String.Format("{0};{1}", "Party GUID", "ModifiedOn (from contact)");
                await WriteCvsLineAsync(outputText);

                // getting a reference to the account based contact dictionnary
                var partyContractDict = _partyContactSrc.GetDictionary();

                // getting a reference to the role based contact dictionnary
                var roleContractDict = _roleContactSrc.GetDictionary();               

                // crossmatching both dictionnaries to find any address mismatch
                foreach (var partyContactEntry in partyContractDict.Values)
                {
                    if (roleContractDict.ContainsKey(partyContactEntry.AccountId))
                    {
                        var roleContactEntry = roleContractDict[partyContactEntry.AccountId];

                        // we are interesed in the address differences
                        if (roleContactEntry != null && roleContactEntry.Address != partyContactEntry.Address)
                        {
                            impactedEntitiesCpt++;
                            outputText = String.Format("{0};{1}", partyContactEntry.AccountId, partyContactEntry.ModifiedOn.ToString("dd-MM-yyyy"));
                            await WriteCvsLineAsync(outputText);
                        }
                    }
                }

                sw.Stop();
                Console.WriteLine("Data Export operation time : {0}", new DateTime(sw.ElapsedTicks).ToString("HH:mm:ss.fff"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ProcessImpactedAccounts] : {0} {1}",
                   ex.Message,
                    ex.InnerException != null ? ex.InnerException.Message : "");
            }

            return impactedEntitiesCpt;
        }      

        public async Task<uint> RetrieveImpactedAccounts()
        {
            uint cptDicrepencies = 0;
            var getContactCountTasks = new List<Task<uint>>();

            _partyContactSrc = new ContactPartySource();
            _roleContactSrc = new ContactRoleSource();            

            _swGlobal.Start();
            _dcrmConnector.Connect();

            // Getting the account attached contacts task
            var contactsFromPartyCpt = _partyContactSrc.GetContactsCountAsync(_partyContactSrc, _dcrmConnector.SrvContext, SOURCE_PARTY);

            // Getting the role attached contacts task
            var contactsFromRoleCpt = _roleContactSrc.GetContactsCountAsync(_roleContactSrc, _dcrmConnector.SrvContext, SOURCE_ROLE);
            
            // Adding the task to task's list
            getContactCountTasks.Add(contactsFromPartyCpt);
            getContactCountTasks.Add(contactsFromRoleCpt);

            Console.WriteLine("Data collection operation ongoing, please wait...\n");

            // We need to wait for all the data collection tasks to be completed before processing the data
            await System.Threading.Tasks.Task.WhenAll(getContactCountTasks);

            if (contactsFromPartyCpt.Status == TaskStatus.RanToCompletion && contactsFromRoleCpt.Status == TaskStatus.RanToCompletion)
            {
                cptDicrepencies = await ProcessImpactedAccounts();
                _swGlobal.Stop();
                Console.WriteLine($"Total execution time  : {new DateTime(_swGlobal.ElapsedTicks).ToString("HH: mm:ss.fff")}");
            }

            return cptDicrepencies;
        }
    }

}

