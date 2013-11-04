using System;
using System.Configuration;

namespace MicroIOC.Configuration
{
	[ConfigurationCollection(typeof(ParameterElement), AddItemName = "parameter")]
	public class ParameterElementCollection : ConfigurationElementCollection
	{
		public ParameterElementCollection()
		{
			this.AddElementName = "parameter";
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new ParameterElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return (element as ParameterElement).Name;
		}

		public new ParameterElement this[string key]
		{
			get { return base.BaseGet(key) as ParameterElement; }
		}

		public ParameterElement this[int ind]
		{
			get { return base.BaseGet(ind) as ParameterElement; }
		}

	}
}