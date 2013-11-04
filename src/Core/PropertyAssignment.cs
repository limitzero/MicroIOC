using System;

namespace MicroIOC.Core
{
	public class PropertyAssignment
	{
		public Type Component { get; private set; }
		public Type PropertyType { get; private set; }
		public string PropertyName { get; private set; }
		public object PropertyValue { get; private set; }

		public PropertyAssignment(Type component, Type propertyType, string propertyName, object propertyValue)
		{
			Component = component;
			PropertyType = propertyType;
			PropertyName = propertyName;
			PropertyValue = propertyValue;
		}
	}
}