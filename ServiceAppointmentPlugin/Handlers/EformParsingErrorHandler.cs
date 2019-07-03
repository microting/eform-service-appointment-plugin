using System.Threading.Tasks;
using eForm.Messages;
using Microting.AppointmentBase.Infrastructure.Data;
using Rebus.Handlers;

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
