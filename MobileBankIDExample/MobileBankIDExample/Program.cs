using System;
using System.Net;
using System.Text.RegularExpressions;
using MobileBankIDExample.BankIDService;

namespace MobileBankIDExample
{
    /// <summary>
    /// Implementation towards the BankID test server
    /// https://github.com/EricHerlitz/Mobile-BankId-.NET-Example
    /// 
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            try
            {
                // Century must be included
                Console.WriteLine("Enter your ssn, 10 or 12 digits (YY)YYMMDDNNNN");

                // format ssn
                string ssn = "197807064337";// GetSsn();

                // authenticate request and return order
                var order = Authenticate(ssn);

                // collect the result
                Collect(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.Read();
            }
        }

        private static OrderResponseType Authenticate(string ssn)
        {
            using (var client = new RpServicePortTypeClient())
            {
                // RequirementType is optional
                // This will ensure only mobile BankID can be used
                // https://www.bankid.com/bankid-i-dina-tjanster/rp-info/guidelines
                RequirementType conditions = new RequirementType
                {
                    condition = new[]
                    {
                        new ConditionType()
                        {
                            key = "certificatePolicies",
                            value = new[] {"1.2.3.4.25"} // Mobile BankID
                        }}
                };

                // Set the parameters for the authentication
                AuthenticateRequestType authenticateRequestType = new AuthenticateRequestType()
                {
                    personalNumber = ssn,
                    requirementAlternatives = new[] { conditions }
                };

                // ...authenticate
                return client.Authenticate(authenticateRequestType);
            }
        }

        private static void Collect(OrderResponseType order)
        {
            using (var client = new RpServicePortTypeClient())
            {
                Console.WriteLine("{0}Start the BankID application and sign in", Environment.NewLine);

                CollectResponseType result = null;

                // Wait for the client to sign in 
                do
                {
                    // ...collect the response
                    result = client.Collect(order.orderRef);
                } while (result.progressStatus != ProgressStatusType.COMPLETE);


                do
                {
                    Console.WriteLine("Hi {0}, please press [ESC] to exit", result.userInfo.givenName);
                } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
            }

        }

        /// <summary>
        /// Method to collect a valid SSN
        /// </summary>
        /// <returns></returns>
        private static string GetSsn()
        {
            string ssn = Console.ReadLine();

            string ssnRegex = @"^(\d{6}|\d{8})[-|(\s)]{0,1}\d{4}$";

            if (string.IsNullOrEmpty(ssn) || !Regex.Match(ssn, ssnRegex).Success)
            {
                throw new ArgumentException("Not a valid SSN");
            }

            // Remove any dash
            ssn = ssn.Replace("-", "");

            // if ten digits we are missing the century
            if (ssn.Length <= 10)
            {
                int year = int.Parse(ssn.Substring(0, 2));
                ssn = year > int.Parse(DateTime.Now.ToString("yy")) ? string.Concat("19", ssn) : string.Concat("20", ssn);
            }

            return ssn;
        }
    }
}
