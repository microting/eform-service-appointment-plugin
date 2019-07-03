using System.Threading.Tasks;
using Microting.AppointmentBase.Infrastructure.Data;
using Rebus.Handlers;
using ServiceAppointmentPlugin.Abstractions;
using ServiceAppointmentPlugin.Messages;

namespace ServiceAppointmentPlugin.Handlers
{
    public class EformCompletedHandler : IHandleMessages<EformCompleted>
    {
        private readonly AppointmentPnDbContext _dbContext;
        private readonly IOutlookOnlineController outlookOnlineController;

        public EformCompletedHandler(AppointmentPnDbContext dbContext, IOutlookOnlineController outlookOnlineController)
        {
            this._dbContext = dbContext;
            this.outlookOnlineController = outlookOnlineController;
        }

#pragma warning disable 1998
        public async Task Handle(EformCompleted message)
        {
            Appointment appo = sqlController.AppointmentFindByCaseId(message.caseId);
            outlookOnlineController.CalendarItemUpdate(appo.GlobalId, appo.Start, ProcessingStateOptions.Completed, appo.Body);
            sqlController.AppointmentsUpdate(appo.GlobalId, ProcessingStateOptions.Completed, appo.Body, "", "", true, appo.Start, appo.End, appo.Duration);
            sqlController.AppointmentSiteUpdate((int)appo.AppointmentSites.First().Id, message.caseId, ProcessingStateOptions.Completed);

        }
    }
}