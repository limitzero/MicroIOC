using System;
using System.Collections.Generic;
using MicroIOC.Core;
using Xunit;

namespace MicroIOC.Tests
{
    public class ContainerTests : IDisposable
    {
        private IContainer _container;
        public static IContainer _container_received;
        public static bool _dispose_called; 

        public ContainerTests()
        {
            _container = new Container();
        }

        public void Dispose()
        {
            if(_container != null)
            {
                _container.Dispose();
            }
            _container = null;

            _container_received = null; 

        }

        [Fact]
        public void it_will_resolve_a_concrete_instance_of_a_type_from_the_container_after_registration()
        {
			using (var container = new Container())
			{
				container.Registrations.Register<IMailService, MailService>();
				container.Registrations.Register<IErrorHandler, ErrorHandler>();
				container.Registrations.Register<ILogger, Logger>();

				var service = container.Resolve<IMailService>();

				Assert.NotNull(service);
				Assert.IsAssignableFrom(typeof(MailService), service);
			}
        }

        [Fact]
        public void can_configure_container_from_defined_installers()
        {
            _container.RegisterFromInstallers(new SampleInstaller());

            var service = _container.Resolve<IMailService>();

            Assert.NotNull(service);
            Assert.IsAssignableFrom(typeof(MailService), service);
        }

		[Fact]
		public void can_configure_container_from_installer_by_type()
		{
			_container.RegisterFromInstaller<SampleInstaller>();

			var service = _container.Resolve<IMailService>();

			Assert.NotNull(service);
			Assert.IsAssignableFrom(typeof(MailService), service);
		}

        [Fact(Skip="App domain issues for this test with runner utility")]
        public void can_automatically_configure_container_from_installers_in_executable_directory()
        {
			using (var container = new Container())
			{
				container.RegisterAllInstallers();
				var service = container.Resolve<IMailService>();

				Assert.NotNull(service);
				Assert.IsAssignableFrom(typeof (MailService), service);
			}
        }

        [Fact]
        public void can_generate_exception_when_resolving_instance_and_dependency_is_not_registered()
        {
            // not registering the concrete implementation for ILogger will generate the exception on resolution
			// for the mail service:
			using(var container = new Container())
			{
				container.Registrations.Register<IMailService, MailService>();
				container.Registrations.Register<IErrorHandler, ErrorHandler>();

				var exception = Assert.Throws<ArgumentException>(() => container.Resolve<IMailService>());
				System.Console.WriteLine(exception.Message);
			}
        
        }

        [Fact]
        public void can_call_dispose_method_on_registered_components_derived_from_IDisposable_when_container_is_disposed()
        {
            using(var container = new Container())
            {
                container.Registrations.Register<IDisposableInstance>(c => new DisposableInstance());
            }

			Assert.True(_dispose_called);
        }

        [Fact]
        public void can_register_instance_as_singleton_and_have_same_component_returned_on_resolution()
        {
            using(var container = new Container())
            {
            	container.Registrations.Register<IDisposableInstance, DisposableInstance>()
                    .WithLifeCycle(ComponentLifeCycles.Singleton);

                var instance = container.Resolve<IDisposableInstance>();
                instance.ID = 1;

                var anotherInstance = container.Resolve<IDisposableInstance>();
                
                Assert.Equal(instance.ID, anotherInstance.ID);
            }
        }

        [Fact]
        public void can_resolve_objects_by_key_from_container()
        {
            string key = typeof (IDisposableInstance).Name;

            using(var container = new Container())
            {
            	container.Registrations.Register<IDisposableInstance, DisposableInstance>(key);
                var instance = container.Resolve(key);

                Assert.NotNull(instance);
            }
        }

        [Fact]
        public void can_resolve_container_for_use_in_other_components_requiring_it_for_processing()
        {
            using (var container = new Container())
            {
            	container.Registrations.Register<IContainerDependentObject, ContainerDependentObject>();
                var instance = container.Resolve<IContainerDependentObject>();

                Assert.NotNull(instance);
                Assert.NotNull(_container_received);
            }
        }

		[Fact]
		public void can_register_contract_and_service_instance_and_resolve_instance_from_contract_via_container()
		{
			using (var container = new Container())
			{
				container.Registrations.Register<IErrorHandler, ErrorHandler>();
				container.Registrations.Register<ILogger, Logger>();
				container.Registrations.Register<IMailService, MailService>();
				var instance = container.Resolve<IMailService>();

				Assert.NotNull(instance);
				Assert.IsAssignableFrom(typeof(MailService), instance);
			}
		}

		[Fact]
		public void can_register_contract_and_service_instance__with_property_value_and_resolve_instance_from_contract_with_parameter_defined_via_container()
		{
			using (var container = new Container())
			{
				container.Registrations.Register<ILogger, Logger>()
					.WithPropertyValue<ILogger>(p => p.LogFileLocation, @"c:\temp");

				var instance = container.Resolve<ILogger>();

				Assert.NotNull(instance);
				Assert.IsAssignableFrom(typeof(Logger), instance);
				Assert.Equal(@"c:\temp", instance.LogFileLocation);
			}
		}

		[Fact]
		public void can_register_component_interface_in_container_with_factory_support_to_create_concrete_instance_outside_of_container()
		{
			using (var container = new Container())
			{
				container.Registrations.RegisterWithFactory<ILogger>(this.CreateLogger);
				var instance = container.Resolve<ILogger>();

				Assert.NotNull(instance);
				Assert.IsAssignableFrom(typeof(Logger), instance);
			}
		}

		[Fact]
		public void can_register_component_and_resolve_by_common_open_type()
		{
			using (var container = new Container())
			{
				container.Registrations.RegisterManyToOpenType(typeof (IHandler<>), this.GetType().Assembly);
				//var instances = container.ResolveAll(typeof (IHandler<Ping>));

				var instances = container.ResolveAll<IHandler<Ping>>();
				var listing = new List<IHandler<Ping>>(instances);

				Assert.IsAssignableFrom(typeof(FirstPingHandler), listing[0]);
				Assert.IsAssignableFrom(typeof(SecondPingHandler), listing[1]);
			}
		}

		[Fact]
		public void can_use_configuration_file_to_configure_container_and_resolve_instance_with_properties_configured()
		{
			using (var container = new Container())
			{
				container.Configure();

				var instance = container.Resolve<ILogger>();

				Assert.NotNull(instance);
				Assert.IsAssignableFrom(typeof(Logger), instance);
				Assert.Equal(@"c:\temp", instance.LogFileLocation);
			}
		}


    	private ILogger CreateLogger()
		{
			return new Logger();
		}

    	public interface IContainerDependentObject
        {
            
        }

        public class ContainerDependentObject : IContainerDependentObject
        {
			// this component may need the container for doing something ?
            public ContainerDependentObject(IContainer container)
            {
                _container_received = container;
            }
        }


        public interface IDisposableInstance : IDisposable
        {
            int ID { get; set; }
        }

        public class DisposableInstance : IDisposableInstance
        {
            public int ID { get; set; }

            public void Dispose()
            {
                _dispose_called = true;
            }

        }

    }

    public class SampleInstaller : IInstaller
    {
        public void Configure(IContainer container)
        {
			container.Registrations.Register<IErrorHandler, ErrorHandler>();
			container.Registrations.Register<ILogger, Logger>();
			container.Registrations.Register<IMailService, MailService>();
        }
    }

    public interface ILogger
    {
		string LogFileLocation { get; set; }
    }

    public class Logger : ILogger
    {
    	public string LogFileLocation { get; set; }
    }

    public interface IMailService
    {
    }

    public interface IErrorHandler
    { }

    public class ErrorHandler : IErrorHandler
    {
    }

    public class MailService : IMailService
    {
        private readonly IErrorHandler _errorHandler;
        private readonly ILogger _logger;

        public MailService(IErrorHandler errorHandler, ILogger logger)
        {
            _errorHandler = errorHandler;
            _logger = logger;
        }
    }


	public interface IHandler<T> where T : class
	{
		void Handle(T message);
	}

	public class Ping {}

	public class FirstPingHandler  : IHandler<Ping>
	{
		private readonly IContainer _container;

		public FirstPingHandler(IContainer container)
		{
			_container = container;
		}

		public void Handle(Ping message)
		{
			
		}
	}

	public class SecondPingHandler : IHandler<Ping>
	{
		public void Handle(Ping message)
		{

		}
	}
}