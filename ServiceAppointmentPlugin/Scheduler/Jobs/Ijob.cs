using System.Threading.Tasks;

namespace ServiceAppointmentPlugin.Scheduler.Jobs
{
    public interface IJob
    {
        Task Execute();
    }
}
