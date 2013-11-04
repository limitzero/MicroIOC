using System;
using System.Configuration;

namespace MicroIOC.Configuration
{
	[ConfigurationCollection(typeof(ComponentElement), AddItemName = "component")]
	public class ComponentElementCollection : ConfigurationElementCollection
	{
		public ComponentElementCollection()
		{
			this.AddElementName = "component";
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new ComponentElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return (element as ComponentElement).Id;
		}

		public new ComponentElement this[string key]
		{
			get { return base.BaseGet(key) as ComponentElement; }
		}

		public ComponentElement this[int ind]
		{
			get { return base.BaseGet(ind) as ComponentElement; }
		}
	}
}