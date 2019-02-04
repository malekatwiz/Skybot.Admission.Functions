using System;

namespace Skybot.Admission.Function
{
    public static class Settings
    {
        public static string SecretKey => GetEnvironmentVariable("SecretKey");
        public static string SkybotAuthClientId => GetEnvironmentVariable("AuthClientId");
        public static string SkybotAuthClientSecret => GetEnvironmentVariable("AuthClientSecret");
        public static string SkybotAuthUri => GetEnvironmentVariable("AuthUri");
        public static string SkybotAccountsUri => GetEnvironmentVariable("AccountsUri");
        public static string ServiceBusConnectionString => GetEnvironmentVariable("ServiceBusConnectionString");

        private static string GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}
