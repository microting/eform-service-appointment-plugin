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
            Appointment appo = Appointment.AppointmentFindByCaseId(_dbContext, message.caseId);
            outlookOnlineController.CalendarItemUpdate(appo.GlobalId, appo.Start, Appointment.ProcessingStateOptions.Retrived, appo.Body);
            Appointment.AppointmentsUpdate(_dbContext, appo.GlobalId, Appointment.ProcessingStateOptions.Retrived, appo.Body, "", "", true, appo.Start, appo.End, appo.Duration);
            AppointmentSite appointmentSite = new AppointmentSite()
            {
                Id = (int)appo.AppointmentSites.First().Id,
                SdkCaseId = message.caseId,
                ProcessingState = Constants.ProcessingState.Retrieved
            };

            await appointmentSite.Update(_dbContext);
//            Appointment.AppointmentSiteUpdate((int)appo.AppointmentSites.First().Id, message.caseId, ProcessingStateOptions.Retrived);

        }
    }
}