using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MicroIOC.Core
{
	public class Kernel : IKernel
	{
		private readonly IContainer _container;
		private static readonly object _nodeLock = new object();
		private bool _disposed;

		public HashSet<Node> Nodes { get; private set; }

		public Kernel(IContainer container)
		{
			this._container = container;
			this.Nodes = new HashSet<Node>();
		}

		~Kernel()
		{
			this.Dispose(true);
		}

		public void CreateNode(Node node)
		{
			if (this._disposed == true) return;

			lock (_nodeLock)
			{
				if (this.Nodes.Contains(node) == false)
				{
					this.Nodes.Add(node);
				}
			}
		}

		public object Resolve(Type component)
		{
			object instance = null;
			Node node = null;

			if (this._disposed == true) return instance;

			if (component.IsInterface == true)
			{
				//node = (from match in Nodes
				//        where match.Contract == component
				//        select match).FirstOrDefault();
				node = this.FindNodeBy(n => n.Contract == component);
			}
			else
			{
				//node = (from match in Nodes
				//        where match.Component == component
				//        select match).FirstOrDefault();
				node = this.FindNodeBy(n => n.Component == component);
			}

			if (node == null)
				throw new ArgumentException(string.Format("No registration found for component '{0}'.",
					component.FullName));

			return GenerateInstance(node);

			//if (node != null)
			//{
			//    // use recursion, first then defer to custom factory method:
			//    if (node.HasFactorySupport == false)
			//    {
			//        if (node.Component != null)
			//        {
			//            if (node.ComponentLifeCycle == ComponentLifeCycles.Transient)
			//            {
			//                instance = ResolveInternal(node.Component);
			//            }
			//            else if (node.ComponentLifeCycle == ComponentLifeCycles.Singleton)
			//            {
			//                if (node.Instance == null)
			//                {
			//                    instance = ResolveInternal(node.Component);
			//                    node.SetInstance(instance);
			//                }
			//                else
			//                {
			//                    instance = node.Instance;
			//                }
			//            }
			//        }
			//    }
			//    else
			//    {
			//        instance = node.GetInstance();
			//    }
			//}

			//return instance;
		}

		public object Resolve(string key)
		{
			object instance = null;

			if (this._disposed == true) return instance;

			var node = this.FindNodeBy(x => x.Key == key);

			if (node == null)
				throw new ArgumentException(string.Format("No registration found for component key '{0}'.",
				   key));

			return this.GenerateInstance(node);

			////var aNode = (from node in Nodes
			////             where node.Key == key
			////             select node).FirstOrDefault();

			//if (node != null)
			//{
			//    // use factory method, first then recursively resolve:
			//    instance = node.GetInstance();

			//    if (instance == null)
			//    {
			//        instance = ResolveInternal(node.Component);
			//    }
			//}
			//else
			//{
			//    throw new ArgumentException(string.Format("No registration found for component key '{0}'.",
			//       key));
			//}

			//return instance;
		}

		public object[] ResolveAll(Type type)
		{
			List<object> instances = new List<object>();
			if (this._disposed == true) return instances.ToArray();

			var selectedTypes = (from match in this.Nodes
								 let selectedType = match.Contract ?? match.Component
								 where type.IsAssignableFrom(match.Component) ||
									   type.IsAssignableFrom(match.Contract)
								 select selectedType).ToList();

			foreach (var selectedType in selectedTypes)
			{
				var instance = this.Resolve(selectedType);

				if (instance != null)
				{
					instances.Add(instance);
				}
			}

			return instances.ToArray();
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private object GenerateInstance(Node node)
		{
			object instance = null;

			// use recursion, first then defer to custom factory method:
			if (node.HasFactorySupport == false)
			{
				if (node.Component != null)
				{
					if (node.ComponentLifeCycle == ComponentLifeCycles.Transient)
					{
						instance = ResolveInternal(node.Component);
					}
					else if (node.ComponentLifeCycle == ComponentLifeCycles.Singleton)
					{
						if (node.Instance == null)
						{
							instance = ResolveInternal(node.Component);
							node.SetInstance(instance);
						}
						else
						{
							instance = node.Instance;
						}
					}
				}
			}
			else
			{
				// factory support:
				instance = node.GetInstance();
			}

			return instance;
		}

		private Node FindNodeBy(Predicate<Node> search)
		{
			return this.Nodes.FirstOrDefault(n => search(n));
		}

		private void Dispose(bool disposing)
		{
			if (disposing == true)
			{
				foreach (Node node in Nodes)
				{
					IDisposable theDisposable = null;

					if (typeof(IDisposable).IsAssignableFrom(node.Component))
					{
						theDisposable = node.GetInstance() as IDisposable;
					}
					else if (node.Instance != null)
					{
						if (typeof(IDisposable).IsAssignableFrom(node.Instance.GetType()))
						{
							theDisposable = node.Instance as IDisposable;
						}
					}

					if (theDisposable != null)
					{
						try
						{
							theDisposable.Dispose();
						}
						catch
						{
							continue;
						}
					}

					node.Dispose();

				}
			}
			this._disposed = true;
		}

		private object ResolveInternal(Type component)
		{
			// find the greediest constructor and begin resolution from there:
			ConstructorInfo componentConstructor =
				component.GetConstructors()
				.OrderByDescending(c => c.GetParameters().Length).Select(c => c).FirstOrDefault();

			List<object> resolvedInstances = new List<object>();

			if (componentConstructor != null)
			{
				foreach (var parameter in componentConstructor.GetParameters())
				{
					if (typeof(IContainer).IsAssignableFrom(parameter.ParameterType) == true)
					{
						resolvedInstances.Add(this._container);
					}
					else
					{
						var dependency = this.Resolve(parameter.ParameterType);
						SetAssignedProperties(dependency);

						resolvedInstances.Add(dependency);

						this.ResolveInternal(dependency.GetType());
					}
				}
			}

			var resolvedInstance = Activator.CreateInstance(component, resolvedInstances.ToArray());
			SetAssignedProperties(resolvedInstance);

			return resolvedInstance;
		}

		private void SetAssignedProperties(object component)
		{
			var node = (from match in this.Nodes
						where match.Component == component.GetType()
							  || match.Contract == component.GetType()
						select match).FirstOrDefault();

			if (node == null) return;

			foreach (var propertyAssignment in node.PropertyAssignments)
			{
				if (propertyAssignment.PropertyValue.GetType() !=
					component.GetType().GetProperty(propertyAssignment.PropertyName).PropertyType)
				{
					throw new InvalidCastException(string.Format("For the component '{0}' with configured property " +
					 "'{1}' for assignment, the current value '{2}({3})' does not match the type '{4}' of the property on the component. " +
					"Please check the assignment statements when registring the component to make sure the value matches the property type.",
																 component.GetType().FullName,
																 propertyAssignment.PropertyName,
																 propertyAssignment.PropertyValue.ToString(),
																 propertyAssignment.PropertyValue.GetType().FullName,
																 component.GetType().GetProperty(propertyAssignment.PropertyName).
																	PropertyType.FullName));
				}

				component.GetType().GetProperty(propertyAssignment.PropertyName)
						.SetValue(component, propertyAssignment.PropertyValue, null);
			}
		}
	}
}