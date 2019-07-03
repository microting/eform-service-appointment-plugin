using System.Threading.Tasks;
using Microting.AppointmentBase.Infrastructure.Data;
using Rebus.Handlers;
using ServiceAppointmentPlugin.Messages;

namespace ServiceAppointmentPlugin.Handlers
{
    public class AppointmentDeletedInOutlookHandler : IHandleMessages<AppointmentDeletedInOutlook>
    {
        private readonly AppointmentPnDbContext _dbContext;

        public AppointmentDeletedInOutlookHandler(AppointmentPnDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

#pragma warning disable 1998
        public async Task Handle(AppointmentDeletedInOutlook message)
        {
        }
    }
}
