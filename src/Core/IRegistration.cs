using System;
using System.Linq.Expressions;
using System.Reflection;

namespace MicroIOC.Core
{
    public interface IRegistration
    {
        /// <summary>
        /// This will register a component in the container with a lamba representing the factory 
        /// method for resolving dependencies..
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        IRegistration Register<TType>(Func<IContainer, TType> func);

        /// <summary>
        /// This will register a component in the container with a lamba representing the factory 
        /// method for resolving dependencies by unique key.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        IRegistration Register<TType>(string key, Func<IContainer, TType> func);

    	/// <summary>
    	/// This will register a concrete service with its corresponding contract (i.e. interface) within the container.
    	/// </summary>
    	/// <typeparam name="TContract">Interface that the concrete class implements</typeparam>
    	/// <typeparam name="TService">Concrete class that uses the interface</typeparam>
    	/// <param name="lifeCycles">Optional. Lifecycle for registering the component.</param>
    	/// <returns></returns>
    	IRegistration Register<TContract, TService>(ComponentLifeCycles lifeCycles = ComponentLifeCycles.Transient)
    		where TService : class, TContract;

		IRegistration Register(string key, Type contract, Type service, ComponentLifeCycles lifeCycles = ComponentLifeCycles.Transient);

        /// <summary>
        /// This will register an instance of the component in the container.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        IRegistration RegisterInstance<TType>(TType instance);

        /// <summary>
        /// This will register an instance of the component in the container by unique key.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="key"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        IRegistration RegisterInstance<TType>(string key, TType instance);

    	/// <summary>
    	/// This will register an instance of the component in the container with an external method 
    	/// for creating the concrete instance.
    	/// </summary>
    	/// <typeparam name="TType">Type of component to create</typeparam>
    	/// <param name="factory">External method used to create the instance.</param>
    	/// <returns></returns>
    	void RegisterWithFactory<TType>(Func<TType> factory);

    	/// <summary>
    	/// This will allow for a type to be inspected and all implementations of the type 
    	/// will be registered in the container.
    	/// </summary>
    	/// <param name="type"></param>
    	/// <param name="assemblyToInspect">Assembly to begin scanning for implementors of type</param>
    	void RegisterManyToOpenType(Type type, Assembly assemblyToInspect);

	    /// <summary>
    	/// This  will assign a property with the value in the expression at resolution of the concrete instance.
    	/// </summary>
    	/// <typeparam name="TComponent">Component to inject property value for at resolution</typeparam>
    	/// <param name="property">Expression denoting the property on the component to set</param>
    	/// <param name="value">Value to be assigned to the property.</param>
    	/// <returns></returns>
    	/// <exception cref="InvalidCastException">Throws exception when type of value defined does not match property type definition</exception>
    	IRegistration WithPropertyValue<TComponent>(Expression<Func<TComponent, object>> property, object value);

		/// <summary>
		/// This  will assign a property with the value in the expression at resolution of the concrete instance.
		/// </summary>
		IRegistration WithPropertyValue(Type component, string propertyName, object value);

        /// <summary>
        /// This will set the lifecycle of the component in the container.
        /// </summary>
        /// <param name="lifeCycle"></param>
        /// <returns></returns>
        IRegistration WithLifeCycle(ComponentLifeCycles lifeCycle);


    	IRegistration Register<TContract, TService>(string key, ComponentLifeCycles lifeCycles = ComponentLifeCycles.Transient)
    		where TService : class, TContract;
    }
}