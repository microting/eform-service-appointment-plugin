using System.Linq;
using System.Threading.Tasks;
using Microting.AppointmentBase.Infrastructure.Data;
using Microting.AppointmentBase.Infrastructure.Data.Constants;
using Rebus.Handlers;
using ServiceAppointmentPlugin.Abstractions;
using ServiceAppointmentPlugin.Infrastructure.Models;
using ServiceAppointmentPlugin.Messages;
using AppointmentSite = Microting.AppointmentBase.Infrastructure.Data.Entities.AppointmentSite;

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
            Appointment appo = Appointment.AppointmentFindByCaseId(_dbContext, message.caseId);
            outlookOnlineController.CalendarItemUpdate(appo.GlobalId, appo.Start, Appointment.ProcessingStateOptions.Completed, appo.Body);
            Appointment.AppointmentsUpdate(_dbContext, appo.GlobalId, Appointment.ProcessingStateOptions.Completed, appo.Body, "", "", true, appo.Start, appo.End, appo.Duration);
            AppointmentSite appointmentSite = new AppointmentSite()
            {
                Id = (int)appo.AppointmentSites.First().Id,
                SdkCaseId = message.caseId,
                ProcessingState = Constants.ProcessingState.Completed
            };

            await appointmentSite.Update(_dbContext);
        }
    }
}