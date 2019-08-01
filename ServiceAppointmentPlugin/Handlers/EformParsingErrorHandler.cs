using System.Threading.Tasks;
using Microting.AppointmentBase.Infrastructure.Data;
using Rebus.Handlers;
using ServiceAppointmentPlugin.Messages;

namespace ServiceAppointmentPlugin.Handlers
{
    public class EformParsingErrorHandler : IHandleMessages<EformParsingError>
    {
        private readonly AppointmentPnDbContext _dbContext;

        public EformParsingErrorHandler(AppointmentPnDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

#pragma warning disable 1998
        public async Task Handle(EformParsingError message)
        {
        }
    }
}
