using System.Threading.Tasks;
using Microting.AppointmentBase.Infrastructure.Data;
using Rebus.Handlers;
using ServiceAppointmentPlugin.Messages;

namespace ServiceAppointmentPlugin.Handlers
{
    public class AppointmentMovedInOutlookHandler : IHandleMessages<AppointmentMovedInOutlook>
    {
        private readonly AppointmentPnDbContext _dbContext;

        public AppointmentMovedInOutlookHandler(AppointmentPnDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

#pragma warning disable 1998
        public async Task Handle(AppointmentMovedInOutlook message)
        {
        }
    }
}
