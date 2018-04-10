using System;
using System.ServiceModel;
using Nito.AsyncEx;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace BatchAddressAnalyzer
{
    class Program
    {
        #region Main
        static public void Main(string[] args)
        {
            AsyncContext.Run(() => MainAsync(args));
        }
        #endregion // Main

        #region GetConfiguration
        static private IConfigurationRoot  GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

            var configuration = builder.Build();

            return configuration;
        }
        #endregion // GetConfiguration

        #region MainAsync
        // We are creating an async main method based on Nito.AsyncEx
        static async void MainAsync(string[] args)
        {
            var partyGuid = args.Length == 1 ? args[0] : null;

            var BatchAddressAnalyzer = new BatchAddressAnalyzer();
            LoadConfiguration();

            try
            {
                Console.WriteLine($"CSV export files location (from appsettings.json) :\n=> {BatchAddressAnalyzer.OutputPath}\n");
                if (!Directory.Exists(BatchAddressAnalyzer.OutputDir))
                    Directory.CreateDirectory(BatchAddressAnalyzer.OutputDir);
                File.Create(BatchAddressAnalyzer.OutputPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CVS export files could not be created : {ex.Message}");
                return;
            }

            try
            {
                if (partyGuid != null)
                {
                    Console.WriteLine($"The application is looking for : {partyGuid} related contacts");
                }

                var cptDicrepencies = await BatchAddressAnalyzer.RetrieveImpactedAccounts(BatchAddressAnalyzer.OutputPath, partyGuid);

                if (cptDicrepencies > 0)
                    Console.WriteLine($"Count of found discrepencies : {cptDicrepencies}");
                else
                    Console.WriteLine("No discrepencie was found");
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
            {
                Console.WriteLine("The application terminated with an error.");
                Console.WriteLine($"Timestamp: { ex.Detail.Timestamp}");
                Console.WriteLine($"Code: {ex.Detail.ErrorCode}");
                Console.WriteLine($"Message: {ex.Detail.Message}");
                Console.WriteLine($"Plugin Trace: {ex.Detail.TraceText}");
                Console.WriteLine($"Inner Fault: { ex.InnerException.Message ?? "No Inner Fault"}");
            }
            catch (System.TimeoutException ex)
            {
                Console.WriteLine("The application terminated with an error.");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                Console.WriteLine($"Inner Fault: { ex.InnerException.Message ?? "No Inner Fault"}");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("The application terminated with an error.");
                Console.WriteLine(ex.Message);

                // Display the details of the inner exception.
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);

                    if (ex.InnerException is FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> fe)
                    {
                        Console.WriteLine($"Timestamp: {fe.Detail.Timestamp}");
                        Console.WriteLine($"Code: {fe.Detail.ErrorCode}");
                        Console.WriteLine($"Message: {fe.Detail.Message}");
                        Console.WriteLine($"Plugin Trace: {fe.Detail.TraceText}");
                        Console.WriteLine("Inner Fault: {0}",
                            null == fe.Detail.InnerFault ? "No Inner Fault" : "Has Inner Fault");
                    }
                }
            }
            finally
            {
                BatchAddressAnalyzer.Terminate();
            }
        }
        #endregion //MainAsync

        private static void LoadConfiguration()
        {
            var configuration = GetConfiguration();
            BatchAddressAnalyzer.OutputDir = configuration["OutputDir"];
            BatchAddressAnalyzer.OutputFile = configuration["OutputFile"];
            BatchAddressAnalyzer.OutputPath = string.Format($@"{BatchAddressAnalyzer.OutputDir}\{BatchAddressAnalyzer.OutputFile}");
            BatchAddressAnalyzer.IsDebugMode = configuration["IsDebugMode"] == "true";
        }
    }
}
