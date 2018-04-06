using System;
using System.ServiceModel;
using Nito.AsyncEx;

namespace AddressAnalyzer
{
    class Program
    {
        static public void Main(string[] args)
        {
            AsyncContext.Run(() => MainAsync(args));
        }

        #region MainAsync
        // We are creating an async main method based on Nito.AsyncEx
        static async void MainAsync(string[] args)
        {
            var partyGuid = args.Length == 1 ? args[0] : null;

            var addressAnalyzer = new AddressAnalyzer();

            try
            {
                if (partyGuid != null)
                {
                    Console.WriteLine($"The application was launched in DEBUG mode : {partyGuid}");
                }

                var cptDicrepencies = await addressAnalyzer.RetrieveImpactedAccounts(partyGuid);

                if (cptDicrepencies > 0)
                    Console.WriteLine($"Count of discrepencies found : {cptDicrepencies}");
                else
                    Console.WriteLine("No where discrepencies found");
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
                addressAnalyzer.Terminate();
            }
        }
        #endregion //MainAsync
    }
}
