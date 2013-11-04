using System.Configuration;

namespace MicroIOC.Configuration
{
	public class ConfigurationSectionHandler : ConfigurationSection
	{
		private const string SectionName = "micro.ioc";

		public static ConfigurationSectionHandler GetConfiguration()
		{
			return (ConfigurationSectionHandler) System.Configuration.ConfigurationManager.GetSection(SectionName);
		}

		[ConfigurationProperty("components", IsDefaultCollection = true, IsRequired = true)]
		public ComponentElementCollection Components
		{
			get
			{
				return (ComponentElementCollection)this["components"] ??
				   new ComponentElementCollection();
			}
		}
	}
}