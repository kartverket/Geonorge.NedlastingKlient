using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace NedlastingKlient
{
    public class ProtectionService
    {
        private static readonly IDataProtector _protector = AddDataProtectionService();

        public static string CreateProtectedPassword(string unprotectedPassword)
        {
            return _protector.Protect(unprotectedPassword);
        }

        private static IDataProtector AddDataProtectionService()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDataProtection();
            var services = serviceCollection.BuildServiceProvider();
            IDataProtector protector = services.GetDataProtector("Auth.Download");
            return protector;
        }

        public static string GetUnprotectedPassword(string protectedPassword)
        {
            return string.IsNullOrEmpty(protectedPassword) ? null : _protector.Unprotect(protectedPassword);
            try
            {
                return _protector.Unprotect(protectedPassword);
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("Data was not encrypted. An error occurred.");
                Console.WriteLine(e.ToString());
                return null;
            }
        }
    }
}
