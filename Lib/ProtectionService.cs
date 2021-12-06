using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using System;
using Serilog;
using System.Reflection;

namespace Geonorge.MassivNedlasting
{
    public class ProtectionService
    {
        private static readonly ILogger Log = Serilog.Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly IDataProtector Protector = AddDataProtectionService();

        public static string CreateProtectedPassword(string unprotectedPassword)
        {
            return Protector.Protect(unprotectedPassword);
        }

        private static IDataProtector AddDataProtectionService()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDataProtection();
            var services = serviceCollection.BuildServiceProvider();
            var protector = services.GetDataProtector("Auth.Download");
            return protector;
        }

        public static string GetUnprotectedPassword(string protectedPassword)
        {
            try { 
                return string.IsNullOrEmpty(protectedPassword) ? null : Protector.Unprotect(protectedPassword);
            }
            catch(Exception ex) 
            {
                Log.Error("Problem getting password", ex);
                return null;
            }
        }
    }
}