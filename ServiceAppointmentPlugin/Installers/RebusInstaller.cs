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
            Configure.With(new CastleWindsorContainerAdapter(container))
                .Logging(l => l.ColoredConsole())
                .Transport(t => t.UseRabbitMq("amqp://admin:password@localhost", "eform-service-appointment-plugin"))
                .Options(o =>
                {
                    o.SetMaxParallelism(_maxParallelism);
                    o.SetNumberOfWorkers(_numberOfWorkers);
                    o.LogPipeline(verbose:true);
                })
                .Start();
        }
    }
}