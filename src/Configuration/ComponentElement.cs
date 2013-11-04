using System.Configuration;

namespace MicroIOC.Configuration
{
	public class ComponentElement : ConfigurationElement
	{
		private const string ElementId = "id";
		private const string ElementContract = "contract";
		private const string ElementService = "service";
		private const string ElementParameters = "parameters";

		[ConfigurationProperty(ElementId, IsRequired = true, IsKey = true)]
		public string Id
		{
			get { return (string)this[ElementId]; }
			set { this[ElementId] = value; }
		}

		[ConfigurationProperty(ElementContract, IsRequired = false, IsKey = false, DefaultValue = "")]
		public string Contract
		{
			get { return (string)this[ElementContract]; }
			set { this[ElementContract] = value; }
		}

		[ConfigurationProperty(ElementService, IsRequired = true, IsKey = false)]
		public string Service
		{
			get { return (string)this[ElementService]; }
			set { this[ElementService] = value; }
		}

		[ConfigurationProperty(ElementParameters, IsDefaultCollection = true, IsRequired = false)]
		public ParameterElementCollection Parameters
		{
			get
			{
				return (ParameterElementCollection)this[ElementParameters] ??
				   new ParameterElementCollection();
			}
		}
	}
}