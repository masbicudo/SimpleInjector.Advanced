using System.Configuration;

namespace SimpleInjector.Advanced.Tests.Models
{
    public class ConfigDepender
    {
        public ConfigDepender(ConnectionStringSettings masb, string masbConnectionString, string masbConnectionProviderName, string masbAppSetting)
        {
            this.Masb = masb;
            this.MasbConnectionProviderName = masbConnectionProviderName;
            this.MasbConnectionString = masbConnectionString;
            this.MasbAppSetting = masbAppSetting;
        }

        public string MasbConnectionString { get; private set; }

        public string MasbConnectionProviderName { get; private set; }

        public string MasbAppSetting { get; private set; }

        public ConnectionStringSettings Masb { get; private set; }
    }
}