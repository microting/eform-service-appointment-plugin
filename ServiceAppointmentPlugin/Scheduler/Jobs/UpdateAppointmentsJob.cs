using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microting.AppointmentBase.Infrastructure.Data;
using Microting.AppointmentBase.Infrastructure.Data.Entities;
using Microting.AppointmentBase.Infrastructure.Data.Enums;
using Microting.eForm.Dto;
using Microting.eForm.Infrastructure;
using Microting.eForm.Infrastructure.Constants;
using Microting.eForm.Infrastructure.Data.Entities;
using Microting.eForm.Infrastructure.Models;
using Rebus.Bus;

namespace ServiceAppointmentPlugin.Scheduler.Jobs
{
    class UpdateAppointmentsJob : IJob
    {
        private readonly AppointmentPnDbContext _dbContext;
        private readonly IBus _bus;
        private readonly eFormCore.Core _core;

        public UpdateAppointmentsJob(AppointmentPnDbContext dbContext, IBus bus, eFormCore.Core core)
        {
            _dbContext = dbContext;
            _bus = bus;
            _core = core;
        }

        public async Task Execute()
        {
            Console.WriteLine("UpdateAppointmentsJob started");

            await UpdateRecurringAppointments();
            await CreateAppointmentsCases();

            Console.WriteLine("UpdateAppointmentsJob finished");
        }

        private async Task UpdateRecurringAppointments()
        {
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
        }

        private async Task CreateAppointmentsCases()
        {
            // Get appointments, that are ready to start
            var appointments = await _dbContext.Appointments.Where(x =>
                x.WorkflowState == Constants.WorkflowStates.Created
                && x.StartAt < DateTime.UtcNow
                && x.ExpireAt > DateTime.UtcNow
            ).ToListAsync();

            Console.WriteLine($"Found {appointments.Count} appointments to start");

            await using MicrotingDbContext microtingDbContext = _core.DbContextHelper.GetDbContext();
            Language language = await microtingDbContext.Languages.SingleAsync(x => x.LanguageCode == "da");
            foreach (var appointment in appointments)
            {
                appointment.WorkflowState = Constants.WorkflowStates.Processed;
                var mainElement = _core.ReadeForm(appointment.SdkeFormId ?? 0, language);

                // appointment can contain prefilled values, so we need to write them to mainElement
                foreach (var fv in appointment.AppointmentPrefillFieldValues)
                {
                    SetDefaultValue(mainElement.Result.ElementList, fv);
                }

                Console.WriteLine($"Appointment {appointment.Id} has {appointment.AppointmentSites.Count} sites associated");

                // create cases to each associated site
                foreach (var appointmentSite in appointment.AppointmentSites)
                {
                    var caseUid = _core.CaseCreate(await mainElement, null, appointmentSite.MicrotingSiteUid, null);
                    Console.WriteLine($"Case {caseUid} created for site {appointmentSite.MicrotingSiteUid}");
                }

                await appointment.Update(_dbContext);
            }
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
        private void SetDefaultValue(IEnumerable<Element> elementLst, AppointmentPrefillFieldValue fv)
        {
            foreach (var element in elementLst)
            {
                if (element is DataElement dataElement)
                {
                    foreach (var item in dataElement.DataItemList.Where(item => fv.FieldId == item.Id))
                    {
                        switch (item)
                        {
                            case NumberStepper numberStepper:
                                numberStepper.DefaultValue = int.Parse(fv.FieldValue);
                                break;
                            case Number number:
                                number.DefaultValue = int.Parse(fv.FieldValue);
                                break;
                            case Comment comment:
                                comment.Value = fv.FieldValue;
                                break;
                            case Text text:
                                text.Value = fv.FieldValue;
                                break;
                            case None none:
                                var cDataValue = new CDataValue();
                                cDataValue.InderValue = fv.FieldValue;
                                none.Description = cDataValue;
                                break;
                            case EntitySearch entitySearch:
                                entitySearch.DefaultValue = int.Parse(fv.FieldValue);
                                break;
                            case EntitySelect entitySelect:
                                entitySelect.DefaultValue = int.Parse(fv.FieldValue);
                                break;

                        }
                    }
                }
                else
                {
                    var groupElement = (GroupElement)element;
                    SetDefaultValue(groupElement.ElementList, fv);
                }
            }
        }
    }
}
