using System;
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
            try
            {
                using (var client = new BankIDService.RpServicePortTypeClient())
                {
                    // Century must be included
                    Console.WriteLine("Enter your ssn, 10 or 12 digits (YY)YYMMDDNNNN");

                    string ssn = GetSsn();

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
                        }
                    }
                    };

                    // Set the parameters for the authentication
                    AuthenticateRequestType authenticateRequestType = new AuthenticateRequestType()
                    {
                        personalNumber = ssn,
                        requirementAlternatives = new[] { conditions }
                    };

                    // ...authenticate
                    OrderResponseType response = client.Authenticate(authenticateRequestType);

                    // Wait for the client to sign in 
                    do
                    {
                        Console.WriteLine("{0}Start the BankID application and sign in, press [ENTER] when done{0}", Environment.NewLine);
                    } while (Console.ReadKey(true).Key != ConsoleKey.Enter);

                    // ...collect the response
                    CollectResponseType result = client.Collect(response.orderRef);

                    do
                    {
                        Console.WriteLine("Hi {0}, please press [ESC] to exit", result.userInfo.givenName);
                    } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.Read();
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
