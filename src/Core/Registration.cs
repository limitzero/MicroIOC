using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MicroIOC.Core
{
    public class Registration  : IRegistration
    {
        private readonly IContainer _container;
        private readonly IKernel _kernel;

        public Registration(IContainer container, IKernel kernel)
        {
            _container = container;
            _kernel = kernel;
        }

        public IRegistration Register<TType>(Func<IContainer, TType> func)
        {
            return this.Register(string.Empty, func);
        }

        public IRegistration Register<TType>(string key, Func<IContainer, TType> func)
        {
            Node node = new Node(_container, typeof(TType), key);

			node.Activate(() => func(_container));

			_kernel.CreateNode(node);

            return this;
        }

    	public IRegistration Register(string key, Type contract, Type service, ComponentLifeCycles lifeCycles)
    	{
			if(service == null)
			{
				throw new InvalidOperationException("The type denoting the concrete service type cannot be null");
			}

    		if(contract != null)
    		{
				if(contract.IsInterface == false)
				{
					throw new InvalidOperationException(new StringBuilder()
							.AppendFormat("The contract '{0}' that is bound to service component '{1}' must be an interface type.",
							contract.Name, service.Name).ToString());
				}

    			if(contract.IsAssignableFrom(service) == false)
				{
					throw new InvalidOperationException(new StringBuilder()
						.AppendFormat("The service '{0}' does not implement the contract '{1}'.", 
						service.Name, contract.Name).ToString());
				}
    		}

    		Node node = null; 

			if(contract != null)
			{ 
				node = new Node(_container, contract, service, key);
			}
			else
			{
				node = new Node(_container,  service, key);
			}
    		
			_kernel.CreateNode(node);

    		return this;
    	}

    	public IRegistration RegisterInstance<TType>(TType instance)
        {
            this.RegisterInstance<TType>(string.Empty, instance);
            return this;
        }

    	public IRegistration Register<TContract, TService>(ComponentLifeCycles lifeCycles) 
			where TService : class, TContract
    	{
    		Node node = new Node(_container, typeof (TContract), typeof (TService));
			_kernel.CreateNode(node);
    		return this;
    	}

		public IRegistration Register<TContract, TService>(string key, ComponentLifeCycles lifeCycles = ComponentLifeCycles.Transient)
		where TService : class, TContract
		{
			Node node = new Node(_container, typeof(TContract), typeof(TService), key);
			node.ComponentLifeCycle = lifeCycles;
			_kernel.CreateNode(node);
			return this;
		}

    	public void RegisterWithFactory<TType>(Func<TType> factory)
    	{
    		Node node = null; 

			if (typeof(TType).IsInterface == true)
			{
				 node = new Node(_container, typeof(TType), null, string.Empty);
			}
			else
			{
				node = new Node(_container, null, typeof(TType), string.Empty);		
			}

    		Func<object> factoryActivation = () => factory();

    		node.HasFactorySupport = true;
			node.Activate(factoryActivation);

    		_kernel.CreateNode(node);
    	}

    	public void RegisterManyToOpenType(Type type, Assembly assemblyToInspect)
    	{
    		var concreteTypes = (from match in assemblyToInspect.GetExportedTypes()
    		             where match.IsClass == true && match.IsAbstract == false
    		             select match).ToList().Distinct();

    		foreach (var concreteType in concreteTypes)
    		{
    			if (concreteType.GetInterfaces().Length > 0)
    			{
    				if (concreteType.GetInterfaces().Any(i => i.FullName.StartsWith(type.FullName)))
    				{
						Node node = new Node(_container, null, concreteType);
						_kernel.CreateNode(node);
    				}
    			}
				else if(type.IsAssignableFrom(concreteType) == true)
				{
					Node node = new Node(_container, null, concreteType);
					_kernel.CreateNode(node);
				}
    		}

    	}

    	public IRegistration WithPropertyValue<TComponent>(Expression<Func<TComponent, object>> property, object value)
    	{
    		var node = (from match in this._kernel.Nodes
    		            where match.Component == typeof (TComponent)
    		                  || match.Contract == typeof (TComponent)
    		            select match).FirstOrDefault();

			if (node == null) return this;

    		var propertyName = GetPropertyName<TComponent>(property);
    		var propertyType = typeof (TComponent).GetProperty(propertyName).PropertyType;
    		var assignment = new PropertyAssignment(typeof (TComponent), propertyType, propertyName, value);
			node.CreatePropertyAssignment(assignment);

    		return this;
    	}

    	public IRegistration WithPropertyValue(Type component, string propertyName, object value)
    	{
			var node = (from match in this._kernel.Nodes
						where match.Component == component
						select match).FirstOrDefault();

			if (node == null) return this;

			if (string.IsNullOrEmpty(propertyName) == false)
			{
				var propertyType = component.GetProperty(propertyName).PropertyType;
				var assignment = new PropertyAssignment(component, propertyType, propertyName, value);
				node.CreatePropertyAssignment(assignment);
			}

    		return this;
    	}

    	public IRegistration RegisterInstance<TType>(string key, TType instance)
        {
            Node node = new Node(_container, typeof(TType), key);

			node.Activate(() => instance);

			_kernel.CreateNode(node);

            return this;
        }

        public IRegistration WithLifeCycle(ComponentLifeCycles lifeCycle)
        {
        	var node = this._kernel.Nodes.LastOrDefault();

			if (node == null)
				throw new InvalidOperationException(
					"You must first specify a component registration via Register(...) before configuring the life cycle of the component in the container.");

			node.ComponentLifeCycle = lifeCycle;

            return this;
        }

		private static string GetPropertyName<TComponent>(Expression<Func<TComponent, object>> expression)
		{
			MemberExpression memberExpression;

			if (expression.Body is UnaryExpression)
			{
				memberExpression = ((UnaryExpression)expression.Body).Operand as MemberExpression;
			}
			else
			{
				memberExpression = expression.Body as MemberExpression;
			}

			if (memberExpression == null)
			{
				throw new InvalidOperationException("You must specify a property!");
			}

			return memberExpression.Member.Name;
		}
    }
}