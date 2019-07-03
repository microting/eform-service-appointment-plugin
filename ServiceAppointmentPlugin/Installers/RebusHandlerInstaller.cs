using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace ServiceAppointmentPlugin.Installers
{
    public class RebusHandlerInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
//            container.Register(Component.For<IHandleMessages<ScheduledItemExecuted>>().ImplementedBy<ScheduledItemExecutedHandler>().LifestyleTransient());
        }
    }
}