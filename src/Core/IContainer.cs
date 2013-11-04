using System;
using System.Collections.Generic;

namespace MicroIOC.Core
{
    /// <summary>
    /// Contract for a simple container for dependency injection/IoC implementation.
    /// </summary>
    public interface IContainer : IDisposable
    {
        /// <summary>
        /// Gets the registration service that will register a component in the container with its dependencies.
        /// </summary>
        IRegistration Registrations { get; }

		/// <summary>
		/// This will configure the container with the components defined in an <seealso cref="IInstaller">installer</seealso>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		void RegisterFromInstaller<T>() where T : class, IInstaller, new();

        /// <summary>
        /// This will configure the container from a list of defined <seealso cref="IInstaller">installers</seealso>
        /// </summary>
        /// <param name="installers"></param>
        void RegisterFromInstallers(params IInstaller[] installers);

        /// <summary>
        /// This will do component scanning to find all concrete types implementing the interface 
        /// <seealso cref="IInstaller"/>, create the component and configure the container from 
        /// the defined dependencies.
        /// </summary>
        void RegisterAllInstallers();

        /// <summary>
        /// This will resolve to a strongly typed instance of an object in the container.
        /// </summary>
        /// <typeparam name="TComponent">Instance to resolve</typeparam>
        /// <returns></returns>
        TComponent Resolve<TComponent>();

        /// <summary>
        /// This will resolve to a loosely typed instance of an object in the container.
        /// </summary>
        /// <param name="component">Type of the component to resolve for.</param>
        /// <returns></returns>
        object Resolve(Type component);

        /// <summary>
        /// This will resolve to a loosely typed instance of an object in the container by unique key.
        /// </summary>
        /// <param name="key">Unique key to search for to create instance.</param>
        /// <returns></returns>
        object Resolve(string key);

		/// <summary>
		/// This will resolve all instances that are assignable from the specified type;
		/// </summary>
		/// <param name="type">Type to interrogate for assignability</param>
		/// <returns></returns>
    	object[] ResolveAll(Type type);

		/// <summary>
		/// This will resolve all instances that are assignable from the specified type;
		/// </summary>
		/// <typeparam name="T">Type to resolve in container</typeparam>
		/// <returns></returns>
		IEnumerable<T> ResolveAll<T>();

		/// <summary>
		/// This will configure the container from the configuration file.
		/// </summary>
    	void Configure();
		

    }
}