using System.Threading.Tasks;
using Microting.AppointmentBase.Infrastructure.Data;
using Rebus.Handlers;
using ServiceAppointmentPlugin.Abstractions;
using ServiceAppointmentPlugin.Messages;

namespace ServiceAppointmentPlugin.Handlers
{
    public class EformRetrievedHandler : IHandleMessages<EformRetrieved>
    {
        private readonly AppointmentPnDbContext _dbContext;
        private readonly IOutlookOnlineController outlookOnlineController;

        public EformRetrievedHandler(AppointmentPnDbContext dbContext, IOutlookOnlineController outlookOnlineController)
        {
            this._dbContext = dbContext;
            this.outlookOnlineController = outlookOnlineController;
        }

#pragma warning disable 1998
        public async Task Handle(EformRetrieved message)
        {
            Appointment appo = sqlController.AppointmentFindByCaseId(message.caseId);
            outlookOnlineController.CalendarItemUpdate(appo.GlobalId, appo.Start, ProcessingStateOptions.Retrived, appo.Body);
            sqlController.AppointmentsUpdate(appo.GlobalId, ProcessingStateOptions.Retrived, appo.Body, "", "", true, appo.Start, appo.End, appo.Duration);
            sqlController.AppointmentSiteUpdate((int)appo.AppointmentSites.First().Id, message.caseId, ProcessingStateOptions.Retrived);

        }
    }
}