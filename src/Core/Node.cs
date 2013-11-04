using System;
using System.Collections.Generic;

namespace MicroIOC.Core
{
	public class Node : IDisposable
	{
		private readonly IContainer _container;
		private Func<object> _activate;
		private bool _disposed;

		public Type Contract { get; private set; }
		public Type Component { get; private set; }
		public object Instance { get; private set; }
		public ComponentLifeCycles ComponentLifeCycle { get; set; }
		public string Key { get; private set; }
		public ICollection<PropertyAssignment> PropertyAssignments { get; private set; }
		public bool HasFactorySupport { get; set; }

		public Node(IContainer container, Type component, string key = "")
			: this(container, null, component, key)
		{
		}

		public Node(IContainer container, Type contract, Type component, string key = "")
		{
			this._container = container;
			this.Contract = contract;
			this.Component = component;
			this.Key = key;
			this.PropertyAssignments = new List<PropertyAssignment>();
		}

		~Node()
		{
			this.Dispose(true);
		}

		public object GetInstance()
		{
			if (this._disposed == true) return null;

			if (this.ComponentLifeCycle == ComponentLifeCycles.Singleton)
			{
				if (this.Instance == null)
				{
					this.Instance = this._activate();
				}

				return this.Instance;
			}
			else
			{
				object instance = null; 

				if(this._activate != null)
				{
					instance = this._activate();
				}

				return instance;
			}
		}

		public void Activate(Func<object> activate)
		{
			if (this._disposed == true) return;
			this._activate = activate;
		}

		public void CreatePropertyAssignment(PropertyAssignment propertyAssignment)
		{
			if (this._disposed == true) return;

			if (this.PropertyAssignments.Contains(propertyAssignment) == false)
			{
				this.PropertyAssignments.Add(propertyAssignment);
			}
		}

		public void SetInstance(object instance)
		{
			this.Instance = instance;
		}
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing == true)
			{
				if (this.Instance != null)
				{
					if (typeof (IDisposable).IsAssignableFrom(this.Instance.GetType()))
					{
						((IDisposable) this.Instance).Dispose();
					}
					this.Instance = null;
				}
			}

			this._disposed = true;
		}


	}
}