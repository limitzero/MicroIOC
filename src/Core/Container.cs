using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using MicroIOC.Configuration;

namespace MicroIOC.Core
{
    public class Container  : IContainer
    {
        private IKernel _kernel;
        private bool _disposed;

        public Container()
        {
            _kernel = new Kernel(this);
        }

		~Container()
		{
			this.Dispose(true);
		}

    	public IRegistration Registrations
        {
            get {return new Registration(this, _kernel);}
        }

        public void RegisterFromInstallers(params IInstaller[] installers)
        {
            foreach (IInstaller installer in installers)
            {
                installer.Configure(this);
            }
        }

		public void RegisterFromInstaller<T>() where T : class, IInstaller, new()
		{
			T installer = new T();
			installer.Configure(this);
		}

    	public void RegisterAllInstallers()
        {
            var files = (from localFile in Directory.GetFiles(System.AppDomain.CurrentDomain.BaseDirectory, "*.dll")
                         where localFile.Contains(GetType().Assembly.FullName) == false
                         select localFile).Distinct().ToList();
            
            foreach(var file in files)
            {
                Assembly asm = null;

                try
                {
                    asm = Assembly.LoadFile(file);
                }
                catch 
                {
                    continue;
                }

                var installerTypes = (from type in asm.GetTypes()
                                  where typeof (IInstaller).IsAssignableFrom(type) &
                                        type.IsClass == true &
                                        type.IsAbstract == false
                                  select type).Distinct().ToList();

                if(installerTypes != null & installerTypes.Count > 0)
                {
                    foreach (Type installerType in installerTypes)
                    {
                        IInstaller installer = asm.CreateInstance(installerType.FullName) as IInstaller;

                        if(installer == null) continue;

                        this.RegisterFromInstallers(installer);
                    }
                }

            }

        }

        public TComponent Resolve<TComponent>()
        {
			GuardOnDispose();

            var aComponent = default(TComponent);

            aComponent = (TComponent) this.Resolve(typeof (TComponent));

            return aComponent;
        }

        public object Resolve(Type component)
        {
			GuardOnDispose();

            return _kernel.Resolve(component);
        }

        public object Resolve(string key)
        {
			GuardOnDispose();

            return this._kernel.Resolve(key);
        }

    	public object[] ResolveAll(Type type)
    	{
			GuardOnDispose();
    		return this._kernel.ResolveAll(type);
    	}

		public IEnumerable<T> ResolveAll<T>()
		{
			GuardOnDispose();
			var types = this._kernel.ResolveAll(typeof (T));

			foreach (var type in types)
			{
				yield return (T)type;
			}
		}

		public void Configure()
		{
			var configuration = ConfigurationSectionHandler.GetConfiguration();

			if(configuration.Components.Count == 0)
				throw new ConfigurationErrorsException("There are no components defined to externally configure the container in the application configuration file.");

			this.RegisterInternal(configuration);
		}

    	private void RegisterInternal(ConfigurationSectionHandler configuration)
    	{
    		foreach (var componentItem in configuration.Components)
    		{
    			var component = componentItem as ComponentElement;

				var registration = this.Registrations.Register(component.Id, 
					FindTypeFromAssemblyName(component.Contract),
					FindTypeFromAssemblyName(component.Service));

				if(component.Parameters.Count > 0)
				{
					foreach (var parameterItem in component.Parameters)
					{
						var parameter = parameterItem as ParameterElement;

						if (parameter != null)
						{
							registration.WithPropertyValue(FindTypeFromAssemblyName(component.Service),
							                               parameter.Name,
							                               parameter.Value);
						}
					}
				}
    		}
    		
    	}

    	public void Dispose()
        {
           Dispose(true);
			GC.SuppressFinalize(this);
        }

		private void Dispose(bool disposing)
		{
			if(disposing == true)
			{
				if (this._kernel != null)
				{
					this._kernel.Dispose();
				}
				this._kernel = null;
			}
			this._disposed = true;
		}

		private void GuardOnDispose()
		{
			if (this._disposed == true) 
				throw new ObjectDisposedException("Can not access a disposed instance of " + this.GetType().FullName);
		}

		private Type FindTypeFromAssemblyName(string assemblyName)
		{
			Assembly asm = null;
			Type component = null; 

			// the pattern for this should be the fully qualified .NET type (namespace to component, assembly name)
			if(assemblyName.Contains(",") == false)
			{
				throw new InvalidOperationException(new StringBuilder()
				                                    	.AppendFormat(
				                                    		"The definition for component '{0}' does not correspond to a .NET component type.",
				                                    		assemblyName).ToString());
			}

			string[] parts = assemblyName.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
			Array.Reverse(parts);

			// load the assembly (if possible):
			try
			{
				asm = Assembly.Load(parts[0].Trim());
			}
			catch (Exception loadAssemblyException)
			{
				string message = new StringBuilder()
					.AppendFormat("An error occurred while attempting to load assembly '{0}'. Reason: '{1}'",
					              parts[0].Trim(), loadAssemblyException.Message).ToString();
				throw new Exception(message, loadAssemblyException);
			}

			// try to load the component (if possible):
			component = asm.GetExportedTypes()
				.Where(t => t.FullName.Equals(parts[1].Trim()))
				.Select(t => t)
				.FirstOrDefault(); 

			if(component == null)
			{
				string message = new StringBuilder()
					.AppendFormat("The type '{0}' could not be found in assembly '{1}'. Please re-check the name of the component in the configuration file.",
								  parts[1].Trim(), parts[0].Trim()).ToString();
				throw new InvalidOperationException(message);
			}

			return component;
		}
    }
}