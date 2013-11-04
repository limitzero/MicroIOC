using System.Configuration;

namespace MicroIOC.Configuration
{
	public class ParameterElement : ConfigurationElement
	{
		private const string ElementKey = "name";
		private const string ElementValue = "value";

		[ConfigurationProperty(ElementKey, IsRequired = true, IsKey = true)]
		public string Name
		{
			get { return (string)this[ElementKey]; }
			set { this[ElementKey] = value; }

		}

		[ConfigurationProperty(ElementValue, IsRequired = true, IsKey = true)]
		public string Value
		{
			get { return (string)this[ElementValue]; }
			set { this[ElementValue] = value; }
		}
	}
}