using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microting.AppointmentBase.Infrastructure.Data;
using Microting.AppointmentBase.Infrastructure.Data.Entities;
using Microting.AppointmentBase.Infrastructure.Data.Enums;
using Microting.eForm.Infrastructure.Constants;
using Rebus.Bus;

namespace ServiceAppointmentPlugin.Scheduler.Jobs
{
    class UpdateAppointmentsJob : IJob
    {
        private readonly AppointmentPnDbContext _dbContext;
        private readonly IBus _bus;

        public UpdateAppointmentsJob(AppointmentPnDbContext dbContext, IBus bus)
        {
            _dbContext = dbContext;
            _bus = bus;
        }

        public async Task Execute()
        {
            Console.WriteLine("UpdateAppointmentsJob started");

            // Get all appointments, that have correct repeat settings and don`t have next appointment associated yet
            var recurringAppointments = await _dbContext.Appointments.Where(x =>
                    x.WorkflowState != Constants.WorkflowStates.Removed
                    && x.RepeatEvery != null
                    && x.RepeatType != null
                    && (x.RepeatUntil == null || x.RepeatUntil > DateTime.UtcNow)
                    && x.StartAt < DateTime.UtcNow
                    && x.NextId == null
                ).ToListAsync();

            Console.WriteLine($"Found {recurringAppointments.Count} appointments to update");

            foreach (var appointment in recurringAppointments)
            {
                var prevAppointment = appointment;

                // Just get some values needed to create next copy of appointment
                var duration = prevAppointment.ExpireAt - prevAppointment.StartAt;
                var repeatEvery = prevAppointment.RepeatEvery.GetValueOrDefault();
                var repeatType = prevAppointment.RepeatType.GetValueOrDefault();
                var repeatUntil = prevAppointment.RepeatUntil;
                var prevDate = prevAppointment.StartAt.GetValueOrDefault();
                var nextDate = GetNextAppointmentDate(prevDate, repeatType, repeatEvery);

                // Clone appointment, until reaching current date, or RepeatUntil date
                while ((repeatUntil == null || nextDate < repeatUntil) && prevDate < DateTime.UtcNow)
                {
                    var nextAppointment = new Appointment
                    {
                        CreatedByUserId = prevAppointment.CreatedByUserId,
                        ExpireAt = nextDate + duration,
                        StartAt = nextDate,
                        Info = prevAppointment.Info,
                        Description = prevAppointment.Description,
                        Title = prevAppointment.Title,
                        ColorHex = prevAppointment.ColorHex,
                        RepeatEvery = prevAppointment.RepeatEvery,
                        RepeatType = prevAppointment.RepeatType,
                        RepeatUntil = prevAppointment.RepeatUntil
                    };

                    await nextAppointment.Create(_dbContext);

                    // Save the Id of new copy of appointment to the NextId property of previous appointment. 
                    prevAppointment.NextId = nextAppointment.Id;
                    await prevAppointment.Update(_dbContext);

                    Console.WriteLine($"Next appointment for {prevAppointment.Id} is {prevAppointment.NextId}");

                    prevDate = nextDate;
                    nextDate = GetNextAppointmentDate(prevDate, repeatType, repeatEvery);
                    prevAppointment = nextAppointment;
                }
            }

            Console.WriteLine("UpdateAppointmentsJob finished");
        }

        private DateTime GetNextAppointmentDate(DateTime prevDate, RepeatType repeatType, int repeatEvery)
        {
            switch (repeatType)
            {
                case RepeatType.Month:
                    return prevDate.AddMonths(repeatEvery);
                case RepeatType.Week:
                    return prevDate.AddDays(repeatEvery * 7);
                default:
                    return prevDate.AddDays(repeatEvery);
            }
        }
    }
}
