using System;
using ServiceAppointmentPlugin.Infrastructure.Models;
using ServiceAppointmentPlugin.OfficeApi;

namespace ServiceAppointmentPlugin.Abstractions
{
    public interface IOutlookOnlineController
    {
        bool CalendarItemConvertRecurrences();

        bool ParseCalendarItems();

        bool CalendarItemReflecting(string globalId);

        Event CalendarItemCreate(string location, DateTime start, int duration, string subject, string body, string originalStartTimeZone, string originalEndTimeZone);

        bool CalendarItemUpdate(string globalId, DateTime start, Appointment.ProcessingStateOptions workflowState, string body);

        bool CalendarItemDelete(string globalId);

        string GetUserEmailAddress();
    }
}