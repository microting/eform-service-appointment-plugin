using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Rebus.Config;

namespace ServiceAppointmentPlugin.Installers
{
    public class RebusInstaller: IWindsorInstaller
    {
        private readonly string _connectionString;
        private readonly int _maxParallelism;
        private readonly int _numberOfWorkers;

        public RebusInstaller(string connectionString, int maxParallelism, int numberOfWorkers)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }
            _connectionString = connectionString;
            _maxParallelism = maxParallelism;
            _numberOfWorkers = numberOfWorkers;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            if (_connectionString.ToLower().Contains("convert zero datetime"))
            {
                Configure.With(new CastleWindsorContainerAdapter(container))
                    .Logging(l => l.ColoredConsole())
                    .Transport(t => t.UseMySql(connectionStringOrConnectionOrConnectionStringName: _connectionString, tableName: "Rebus", inputQueueName: "appointment-input"))
                    .Options(o =>
                    {
                        o.SetMaxParallelism(_maxParallelism);
                        o.SetNumberOfWorkers(_numberOfWorkers);
                    })
                    .Start();
            }
            else
            {
                Configure.With(new CastleWindsorContainerAdapter(container))
                    .Logging(l => l.ColoredConsole())
                    .Transport(t => t.UseSqlServer(connectionString: _connectionString, inputQueueName: "appointment-input"))
                    .Options(o =>
                    {
                        o.SetMaxParallelism(_maxParallelism);
                        o.SetNumberOfWorkers(_numberOfWorkers);
                    })
                    .Start();
            }
        }
    }
}