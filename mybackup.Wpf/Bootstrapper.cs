using Caliburn.Micro;
using mybackup.Wpf.ViewModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace mybackup.Wpf
{
    /// <summary>
    /// The entry point of the MVVM application.
    /// </summary>
    internal class Bootstrapper : BootstrapperBase
    {
        #region Fields

        private readonly SimpleContainer _container = new SimpleContainer();

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="Bootstrapper"/> instance.
        /// </summary>
        public Bootstrapper()
        {
            Initialize();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates and configure the logging service.
        /// </summary>
        /// <returns></returns>
        private ILogger CreateLogger()
        {
            // Configure logger
            return new LoggerConfiguration()
                .MinimumLevel.Error()
                .WriteTo.File("logs/errors.log", rollOnFileSizeLimit: true, fileSizeLimitBytes: 7000000)
                .CreateLogger();
        }
        /// <summary>
        /// Configures the framework and setup the IoC container.
        /// </summary>
        protected override void Configure()
        {
            // Register the IoC container itself
            _container.Instance(_container);

            // Register services
            _container
                .Singleton<IWindowManager, WindowManager>()
                .Singleton<IEventAggregator, EventAggregator>();

            // Register logger
            _container.Instance(CreateLogger());
            // Register viewmodels
            foreach (var assembly in SelectAssemblies())
            {
                assembly.GetTypes()
                    .Where(type => type.IsClass)
                    .Where(type => type.Name.EndsWith("ViewModel"))
                    .ToList()
                    .ForEach(viewModelType => _container.RegisterPerRequest(viewModelType, viewModelType.ToString(), viewModelType));
            }
        }

        /// <summary>
        /// Gets an instance of a service.
        /// </summary>
        /// <param name="service">The service to locate.</param>
        /// <param name="key">The key to locate.</param>
        /// <returns>The located service.</returns>
        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        /// <summary>
        /// Gets all the instances of a service.
        /// </summary>
        /// <param name="service">The service to locate.</param>
        /// <returns>The located services.</returns>
        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        /// <summary>
        /// Injects the services to an object instance.
        /// </summary>
        /// <param name="instance">The instance to perform injection on.</param>
        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        #endregion

        #region Events

        /// <summary>
        /// Executes after the application starts.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            await DisplayRootViewForAsync<ShellViewModel>();
        }

        /// <summary>
        /// Executes after an unhandled exception occurs.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            MessageBox.Show(e.Exception.Message, "An error has occurred.", MessageBoxButton.OK);
        }

        #endregion
    }
}
