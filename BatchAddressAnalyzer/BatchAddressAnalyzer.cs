using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

namespace BatchAddressAnalyzer
{
    class BatchAddressAnalyzer
    {
        #region Properties
        private readonly DcrmConnector _dcrmConnector = new DcrmConnector();
        private readonly Stopwatch _swGlobal = new Stopwatch();
        private StreamWriter _streamWriter = null;
        private IContactSource _partyContactSrc = null;
        private IContactSource _roleContactSrc = null;

        // static members
        private static string SOURCE_PARTY = "Party";
        private static string SOURCE_ROLE = "Role";
        #endregion Properties

        #region BatchAddressAnalyzer
        public BatchAddressAnalyzer()
        {
            _dcrmConnector = new DcrmConnector();
        }
        #endregion //BatchAddressAnalyzer

        #region Terminate
        public void Terminate()
        {
            if (_dcrmConnector != null)
                _dcrmConnector.Disconnect();

            Console.WriteLine("Press <Enter> to exit.");
            Console.ReadLine();
        }
        #endregion //Terminate

        #region WriteCvsLineAsync
        async Task<bool> WriteCvsLineAsync(string CsvOutputFilename, string outputText)
        {
            bool bSuccess = false;

            try
            {
                if (_streamWriter == null)
                {
                    Console.WriteLine($"Exporting the impacted parties to : {CsvOutputFilename} ...");
                    _streamWriter = new StreamWriter(CsvOutputFilename);
                    _streamWriter.AutoFlush = true;
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
        #endregion //WriteCvsLineAsync

        #region RetrieveImpactedAccounts
        public async Task<uint> RetrieveImpactedAccounts(string OutputPath, string partyGuid = null)
        {
            uint cptDicrepencies = 0;
            var getContactCountTasks = new List<Task<uint>>();

            _partyContactSrc = new ContactPartySource();
            _roleContactSrc = new ContactRoleSource();

            if (_partyContactSrc == null || _roleContactSrc == null)
            {
                Console.WriteLine("RetrieveImpactedAccounts() : Could not properly setup the Contact sources.");
                return cptDicrepencies;
            }

            var partyContactSrc = (ContactPartySource)_partyContactSrc;
            var roleContactSrc = (ContactRoleSource)_roleContactSrc;

            _swGlobal.Start();
            _dcrmConnector.Connect();

            // Getting the account attached contacts task
            var contactsFromPartyCpt = partyContactSrc.GetContactsCountAsync(_partyContactSrc, _dcrmConnector.SrvContext, SOURCE_PARTY, partyGuid);

            // Getting the role attached contacts task
            var contactsFromRoleCpt = roleContactSrc.GetContactsCountAsync(_roleContactSrc, _dcrmConnector.SrvContext, SOURCE_ROLE, partyGuid);

            // Adding the task to task's list
            getContactCountTasks.Add(contactsFromPartyCpt);
            getContactCountTasks.Add(contactsFromRoleCpt);

            // We need to wait for all the data collection tasks to be completed before processing the data
            await System.Threading.Tasks.Task.WhenAll(getContactCountTasks);

            if (contactsFromPartyCpt.Status == TaskStatus.RanToCompletion && contactsFromRoleCpt.Status == TaskStatus.RanToCompletion)
            {
                cptDicrepencies = await ProcessImpactedAccounts(OutputPath);
                _swGlobal.Stop();
                Console.WriteLine($"Total execution time  : {new DateTime(ticks: _swGlobal.ElapsedTicks).ToString("HH: mm:ss.fff")}");
            }

            return cptDicrepencies;
        }
        #endregion //RetrieveImpactedAccounts

        #region ProcessImpactedAccounts
        public async Task<uint> ProcessImpactedAccounts(string OutputPath)
        {
            uint impactedEntitiesCpt = 0;

            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                // Creating Export file with CVS Header [GUID | MODIFIED ON]
                var colomn1 = "Account GUID";
                var colomn2 = "ModifiedOn(from contact)";
                var colomn3 = "IsAddressEmpty";

                var outputText = String.Format($"{colomn1};{colomn2};{colomn3}");
                await WriteCvsLineAsync(OutputPath, outputText);

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
                        if (roleContactEntry != null && roleContactEntry.HashedAddress != partyContactEntry.HashedAddress)
                        {
                            impactedEntitiesCpt++;
                            outputText = String.Format($"{partyContactEntry.AccountId};{partyContactEntry.ModifiedOn.ToString("dd-MM-yyyy")};{roleContactEntry.IsFromEmptyAddress}");
                            await WriteCvsLineAsync(OutputPath, outputText);
                        }
                    }
                }

                sw.Stop();
                Console.WriteLine($"Data Export operation time : {new DateTime(sw.ElapsedTicks).ToString("HH: mm:ss.fff")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ProcessImpactedAccounts] : {0} {1}",
                   ex.Message,
                    ex.InnerException != null ? ex.InnerException.Message : "");
            }
            finally
            {
                if (_streamWriter != null)
                    _streamWriter.Close();
            }

            return impactedEntitiesCpt;
        }
        #endregion //ProcessImpactedAccounts
    }
}

