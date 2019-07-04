
using System;
using System.Collections.Generic;
using System.Linq;
using Microting.AppointmentBase.Infrastructure.Data;
using Rebus.Bus;
using ServiceAppointmentPlugin.Abstractions;
using ServiceAppointmentPlugin.Infrastructure.Models;
using ServiceAppointmentPlugin.Messages;
using ServiceAppointmentPlugin.OfficeApi;

namespace ServiceAppointmentPlugin.Services
{
    public class OutlookOnlineController : IOutlookOnlineController
    {
        #region var
        private string _calendarName;
//        SqlController sqlController;
        private readonly AppointmentPnDbContext _dbContext;
//        Log log;
        private readonly Tools _t = new Tools();
        private readonly object _lockOutlook = new object();
        private readonly IBus _bus;

        private readonly OutlookExchangeOnlineAPIClient _outlookExchangeOnlineApiClient;
        private string _userEmailAddress;
        #endregion

        #region con
        public OutlookOnlineController(AppointmentPnDbContext dbContext, OutlookExchangeOnlineAPIClient outlookExchangeOnlineAPIClient, IBus bus)
        {
            this._dbContext = dbContext;
//            this.log = log;
            this._outlookExchangeOnlineApiClient = outlookExchangeOnlineAPIClient;
            this._bus = bus;
        }
        #endregion

        #region public
        public bool CalendarItemConvertRecurrences()
        {
            //return false;
            try
            {
                bool ConvertedAny = false;
                #region var
                //DateTime checkLast_At = DateTime.Parse(sqlController.SettingRead(Settings.checkLast_At));
                double checkPreSend_Hours = double.Parse(_dbContext.PluginConfigurationValues
                    .SingleOrDefault(x => x.Name == "AppointmentBaseSettings:checkPreSend_Hours")?.Value);
                double checkRetrace_Hours = double.Parse(_dbContext.PluginConfigurationValues
                    .SingleOrDefault(x => x.Name == "AppointmentBaseSettings:checkRetrace_Hours")?.Value);
                int checkEvery_Mins = int.Parse(_dbContext.PluginConfigurationValues
                    .SingleOrDefault(x => x.Name == "AppointmentBaseSettings:checkEvery_Mins")?.Value);
                bool includeBlankLocations = bool.Parse(_dbContext.PluginConfigurationValues
                    .SingleOrDefault(x => x.Name == "AppointmentBaseSettings:includeBlankLocations")?.Value);
                
//                double checkPreSend_Hours = double.Parse(sqlController.SettingRead(Settings.checkPreSend_Hours));
//                double checkRetrace_Hours = double.Parse(sqlController.SettingRead(Settings.checkRetrace_Hours));
//                int checkEvery_Mins = int.Parse(sqlController.SettingRead(Settings.checkEvery_Mins));
//                bool includeBlankLocations = bool.Parse(sqlController.SettingRead(Settings.includeBlankLocations));

                DateTime timeOfRun = DateTime.Now;
                DateTime tLimitTo = timeOfRun.AddHours(+checkPreSend_Hours);
                DateTime tLimitFrom = timeOfRun.AddHours(-checkRetrace_Hours);
                #endregion

                #region convert recurrences
                List<Event> eventList = GetCalendarItems(tLimitFrom, tLimitTo);
                if (eventList != null)
                {
                    foreach (Event item in eventList)
                    {
                        if (item.Type.Equals("Occurrence")) //is recurring, otherwise ignore
                        {
                            #region location "planned"?
                            string location = null;
                            try
                            {
                                location = item.Location.DisplayName;
                            }
                            catch (Exception ex)
                            {
//                                log.LogEverything(t.GetMethodName("OutlookOnlineController"), "got exception :" + ex.Message + " when trying to do item.Location.DisplayName for item with id : " + item.Id);
                                return false;
                            }


                            if (string.IsNullOrEmpty(location))
                            {
                                if (includeBlankLocations)
                                    location = "planned";
                                else
                                    location = "";
                            }

                            location = location.ToLower();
                            #endregion
                            Event appointmentGuid = null;
                            if (location == "planned")
                            #region ...
                            {
                                try
                                {
                                    double duration = (item.End.DateTime - item.Start.DateTime).TotalMinutes;
                                    appointmentGuid = CalendarItemCreate(location, item.Start.DateTime, (int)Math.Round(duration), item.Subject,
                                    item.BodyPreview, item.OriginalStartTimeZone, item.OriginalEndTimeZone);
                                }
                                catch (Exception ex)
                                {
//                                    log.LogEverything(t.GetMethodName("OutlookOnlineController"), "got exception :" + ex.Message + " when trying to do CalendarItemCreate for item with id : " + item.Id);
                                    return false;
                                }

                                if (CalendarItemDelete(item.Id))
                                {
//                                    log.LogStandard(t.GetMethodName("OutlookOnlineController"), "converted " + item.Subject + " / " + item.Start.DateTime + "/ duration "+ (item.End.DateTime - item.Start.DateTime).Minutes + " to non-recurence appointment");
                                    _bus.SendLocal(new ParseOutlookItem(appointmentGuid)).Wait();
                                    ConvertedAny = true;
                                }

                            }
                            #endregion
                        }
                    }
                }
                else { }

                #endregion

//                if (ConvertedAny)
//                    log.LogStandard(t.GetMethodName("OutlookOnlineController"), "completed + converted appointment(s)");
//                else
//                    log.LogEverything(t.GetMethodName("OutlookOnlineController"), "completed");

                return ConvertedAny;
            }
            catch (Exception ex)
            {
                throw new Exception(_t.GetMethodName("OutlookOnlineController") + " failed", ex);
            }
        }

        public bool ParseCalendarItems()
        {
            try
            {
                bool AllParsed = false;
                #region var
                //DateTime checkLast_At = DateTime.Parse(sqlController.SettingRead(Settings.checkLast_At));
//                double checkPreSend_Hours = double.Parse(sqlController.SettingRead(Settings.checkPreSend_Hours));
//                double checkRetrace_Hours = double.Parse(sqlController.SettingRead(Settings.checkRetrace_Hours));
//                int checkEvery_Mins = int.Parse(sqlController.SettingRead(Settings.checkEvery_Mins));
//                bool includeBlankLocations = bool.Parse(sqlController.SettingRead(Settings.includeBlankLocations));
                double checkPreSend_Hours = double.Parse(_dbContext.PluginConfigurationValues
                    .SingleOrDefault(x => x.Name == "AppointmentBaseSettings:checkPreSend_Hours")?.Value);
                double checkRetrace_Hours = double.Parse(_dbContext.PluginConfigurationValues
                    .SingleOrDefault(x => x.Name == "AppointmentBaseSettings:checkRetrace_Hours")?.Value);
                int checkEvery_Mins = int.Parse(_dbContext.PluginConfigurationValues
                    .SingleOrDefault(x => x.Name == "AppointmentBaseSettings:checkEvery_Mins")?.Value);
                bool includeBlankLocations = bool.Parse(_dbContext.PluginConfigurationValues
                    .SingleOrDefault(x => x.Name == "AppointmentBaseSettings:includeBlankLocations")?.Value);

                DateTime timeOfRun = DateTime.Now;
                DateTime tLimitTo = timeOfRun.AddHours(+checkPreSend_Hours);
                DateTime tLimitFrom = timeOfRun.AddHours(-checkRetrace_Hours);
                #endregion

                #region process appointments
                List<Event> eventList = GetCalendarItems(tLimitFrom, tLimitTo);
                if (eventList == null)
                {
                    AllParsed = true;
                }
                else
                {
                    foreach (Event item in eventList)
                    {
                        if (item.Type == "SingleInstance") //is NOT recurring, otherwise ignore
                        {
                            if (string.IsNullOrEmpty(item.Location.DisplayName))
                            {
                                _bus.SendLocal(new ParseOutlookItem(item)).Wait();
                            }                         
                        }
                    }
                }
                #endregion

                return AllParsed;
            }
            catch (Exception ex)
            {
//                log.LogException(t.GetMethodName("OutlookOnlineController"),  "failed", ex, false);
                throw new Exception(_t.GetMethodName("OutlookOnlineController") + " failed", ex);
            }
        }

        public bool CalendarItemReflecting(string globalId)
        {
            try
            {
                #region appointment = 'find one';
                Microting.AppointmentBase.Infrastructure.Data.Entities.Appointment appointment = null;
                string Categories = null;
                if (globalId == null)
                    appointment = Appointment.AppointmentsFindOne(_dbContext, 0);
                //else
                //    appointment = sqlController.AppointmentsFind(globalId);

                if (appointment == null) //double checks status if no new found
                    appointment = Appointment.AppointmentsFindOne(_dbContext, 0);
                #endregion

                if (appointment == null)
                    return false;
//                log.LogVariable(t.GetMethodName("OutlookOnlineController"), nameof(appointments), appointment.ToString());

                Event item = AppointmentItemFind(appointment.GlobalId, appointment.StartAt.Value.AddHours(-36), appointment.StartAt.Value.AddHours(36)); // TODO!
                if (item != null)
                {
                    item.Location.DisplayName = appointment.ProcessingState;
                    #region item.Categories = 'workflowState'...
                    switch (appointment.ProcessingState)
                    {
                        case "Planned":
                            Categories = null;
                            break;
                        case "Processed":
                            Categories = CalendarItemCategory.Processing.ToString();
                            break;
                        case "Created":
                            Categories = CalendarItemCategory.Processing.ToString();
                            break;
                        case "Sent":
                            Categories = CalendarItemCategory.Sent.ToString();
                            break;
                        case "Retrived":
                            Categories = CalendarItemCategory.Retrived.ToString();
                            break;
                        case "Completed":
                            Categories = CalendarItemCategory.Completed.ToString();
                            break;
                        case "Canceled":
                            Categories = CalendarItemCategory.Revoked.ToString();
                            break;
                        case "Revoked":
                            Categories = CalendarItemCategory.Revoked.ToString();
                            break;
                        case "Exception":
                            Categories = CalendarItemCategory.Error.ToString();
                            break;
                        case "Failed_to_intrepid":
                            Categories = CalendarItemCategory.Error.ToString();
                            break;
                        default:
                            Categories = CalendarItemCategory.Error.ToString();
                            break;
                    }
                    #endregion
                    item.BodyPreview = appointment.Body;
                    #region item.Body = appointment.expectionString + item.Body + appointment.response ...
                    if (!string.IsNullOrEmpty(appointment.Response))
                    {
//                        if (t.Bool(sqlController.SettingRead(Settings.responseBeforeBody)))
//                        {
//                            item.BodyPreview = "<<< Response: Start >>>" +
//                            Environment.NewLine +
//                            Environment.NewLine + appointment.response +
//                            Environment.NewLine +
//                            Environment.NewLine + "<<< Response: End >>>" +
//                            Environment.NewLine +
//                            Environment.NewLine + item.BodyPreview;
//                        }
//                        else
//                        {
                        item.BodyPreview = item.BodyPreview +
                        Environment.NewLine +
                        Environment.NewLine + "<<< Response: Start >>>" +
                        Environment.NewLine +
                        Environment.NewLine + appointment.Response +
                        Environment.NewLine +
                        Environment.NewLine + "<<< Response: End >>>";
//                        }
                    }
                    if (!string.IsNullOrEmpty(appointment.ExceptionString))
                    {
                        item.BodyPreview = "<<< Exception: Start >>>" +
                        Environment.NewLine +
                        Environment.NewLine + appointment.ExceptionString +
                        Environment.NewLine +
                        Environment.NewLine + "<<< Exception: End >>>" +
                        Environment.NewLine +
                        Environment.NewLine + item.BodyPreview;
                    }
                    #endregion
                    Event eresult = _outlookExchangeOnlineApiClient.UpdateEvent(_userEmailAddress, item.Id, CalendarItemUpdateBody(item.BodyPreview, item.Location.DisplayName, Categories));
                    if (eresult == null)
                    {
                        return false;
                    }
                    else
                    {
//                        log.LogStandard(t.GetMethodName("OutlookOnlineController"), "globalId:'" + appointment.global_id + "' reflected in calendar");
                    }


                }
//                else
//                    log.LogWarning(t.GetMethodName("OutlookOnlineController"), "globalId:'" + appointment.global_id + "' no longer in calendar, so hence is considered to be reflected in calendar");

                Appointment.AppointmentsReflected(_dbContext, appointment.GlobalId);
//                log.LogStandard(t.GetMethodName("OutlookOnlineController"), "globalId:'" + appointment.global_id + "' reflected in database");
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(_t.GetMethodName("OutlookOnlineController") + " failed", ex);
            }
        }

        public Event CalendarItemCreate(string location, DateTime start, int duration, string subject, string body, string originalStartTimeZone, string originalEndTimeZone)
        {
            if (string.IsNullOrEmpty(location))
                throw new ArgumentNullException("location cannot be null or empty");
            if (string.IsNullOrEmpty(subject))
                throw new ArgumentNullException("subject cannot be null or empty");
            if (string.IsNullOrEmpty(body))
                throw new ArgumentNullException("body cannot be null or empty");
            if (string.IsNullOrEmpty(originalStartTimeZone))
                throw new ArgumentNullException("originalStartTimeZone cannot be null or empty");
            if (string.IsNullOrEmpty(originalEndTimeZone))
                throw new ArgumentNullException("originalEndTimeZone cannot be null or empty");
            try
            {
                Event newAppo = _outlookExchangeOnlineApiClient.CreateEvent(_userEmailAddress, GetCalendarID(), CalendarItemCreateBody(subject, body, location, start, start.AddMinutes(duration), originalStartTimeZone, originalEndTimeZone));
//                log.LogStandard(t.GetMethodName("OutlookOnlineController"), "Calendar item created in " + calendarName);
                return newAppo;
            }
            catch (Exception ex)
            {
                throw new Exception(_t.GetMethodName("OutlookOnlineController") + " failed", ex);
            }
        }

        public bool CalendarItemUpdate(string globalId, DateTime start, Appointment.ProcessingStateOptions workflowState, string body)
        {
            if (string.IsNullOrEmpty(globalId))
                throw new ArgumentNullException("globalId cannot be null or empty");
            if (string.IsNullOrEmpty(body))
                throw new ArgumentNullException("body cannot be null or empty");
//            log.LogStandard(t.GetMethodName("OutlookOnlineController"), "CalendarItemUpdate incoming start is : " + start.ToString());
//            log.LogStandard(t.GetMethodName("OutlookOnlineController"), "CalendarItemUpdate incoming globalId is : " + globalId);
            //Event item = AppointmentItemFind(globalId, start.AddHours(-36), start.AddHours(36)); // TODO!
            //Event item = GetEvent(globalId);
            //userEmailAddess = GetUserEmailAddress();
            Event item = _outlookExchangeOnlineApiClient.GetEvent(globalId, _userEmailAddress);

            if (item == null)
                return false;

            item.BodyPreview = body;
            item.Location.DisplayName = workflowState.ToString();
            string Categories = null;
            #region item.Categories = 'workflowState'...
            switch (workflowState)
            {
                case Appointment.ProcessingStateOptions.Planned:
                    Categories = null;
                    break;
                case Appointment.ProcessingStateOptions.Processed:
                    Categories = CalendarItemCategory.Processing.ToString();
                    break;
                case Appointment.ProcessingStateOptions.Created:
                    Categories = CalendarItemCategory.Processing.ToString();
                    break;
                case Appointment.ProcessingStateOptions.Sent:
                    Categories = CalendarItemCategory.Sent.ToString();
                    break;
                case Appointment.ProcessingStateOptions.Retrived:
                    Categories = CalendarItemCategory.Retrived.ToString();
                    break;
                case Appointment.ProcessingStateOptions.Completed:
                    Categories = CalendarItemCategory.Completed.ToString();
                    break;
                case Appointment.ProcessingStateOptions.Canceled:
                    Categories = CalendarItemCategory.Revoked.ToString();
                    break;
                case Appointment.ProcessingStateOptions.Revoked:
                    Categories = CalendarItemCategory.Revoked.ToString();
                    break;
                case Appointment.ProcessingStateOptions.Exception:
                    Categories = CalendarItemCategory.Error.ToString();
                    break;
                case Appointment.ProcessingStateOptions.ParsingFailed:
                    Categories = CalendarItemCategory.Error.ToString();
                    break;
            }
            #endregion

            Event eresult = _outlookExchangeOnlineApiClient.UpdateEvent(_userEmailAddress, item.Id, CalendarItemUpdateBody(item.BodyPreview, item.Location.DisplayName, Categories));
            if (eresult == null)
            {
//                log.LogStandard(t.GetMethodName("OutlookOnlineController"), AppointmentPrint(item) + " NOT updated to " + workflowState.ToString());
                return false;
            }
            else
            {
//                log.LogStandard(t.GetMethodName("OutlookOnlineController"), AppointmentPrint(item) + " updated to " + workflowState.ToString());
                return true;
            }

        }

        public bool CalendarItemDelete(string globalId)
        {
            if (string.IsNullOrEmpty(globalId))
                throw new ArgumentNullException("globalId cannot be null or empty");
//            log.LogEverything(t.GetMethodName("OutlookOnlineController"), "OutlookOnlineController.CalendarItemDelete called");
            try
            {
                _userEmailAddress = GetUserEmailAddress();
                _outlookExchangeOnlineApiClient.DeleteEvent(_userEmailAddress, globalId);
//                log.LogStandard(t.GetMethodName("OutlookOnlineController"), "globalId:'" + globalId + "' deleted");
                return true;
            }
            catch (Exception ex)
            {
//                log.LogStandard(t.GetMethodName("OutlookOnlineController"), "CalendarItemDelete failed got exception:" + ex.Message);
                return false;
            }

        }
        //#endregion

        #region private
        private DateTime RoundTime(DateTime dTime)
        {
            dTime = dTime.AddMinutes(1);
            dTime = new DateTime(dTime.Year, dTime.Month, dTime.Day, dTime.Hour, 0, 0);
//            log.LogVariable(t.GetMethodName("OutlookOnlineController"), nameof(dTime), dTime);
            return dTime;
        }

        private Event GetEvent(string globalId)
        {
            if (string.IsNullOrEmpty(globalId))
                throw new ArgumentNullException("globalId cannot be null or empty");
            try
            {
//                log.LogEverything(t.GetMethodName("OutlookOnlineController"), "OutlookOnlineController.GetEvent called");
                _userEmailAddress = GetUserEmailAddress();
                return _outlookExchangeOnlineApiClient.GetEvent(_userEmailAddress, globalId);

            }
            catch (Exception ex)
            {
                throw new Exception(_t.GetMethodName("OutlookOnlineController") + " failed. Due to no match found global id:" + globalId, ex);
            }
        }

        private Event AppointmentItemFind(string globalId, DateTime tLimitFrom, DateTime tLimitTo)
        {
            if (string.IsNullOrEmpty(globalId))
                throw new ArgumentNullException("globalId cannot be null or empty");
            try
            {
//                log.LogEverything(t.GetMethodName("OutlookOnlineController"), "OutlookOnlineController.AppointmentItemFind called");
                string filter = "AppointmentItemFind [After] '" + tLimitFrom.ToString("g") + "' AND [before] <= '" + tLimitTo.ToString("g") + "'";
//                log.LogVariable(t.GetMethodName("OutlookOnlineController"), nameof(filter), filter.ToString());

                List<Event> calendarItemsAllToday = GetCalendarItems(tLimitFrom, tLimitTo);
                foreach (Event item in calendarItemsAllToday)
                {
                    //log.LogEverything(t.GetMethodName("OutlookOnlineController"), "OutlookOnlineController.AppointmentItemFind calendarItemsAllToday current item.Id is " + item.Id);
                    if (item.Id.Equals(globalId))
                        return item;
                }

                //List<Event> calendarItemsRes = GetCalendarItems(new DateTime(1975, 1, 1).Date, DateTime.Today.AddDays(1));
                List<Event> calendarItemsRes = GetCalendarItems(new DateTime(1975, 1, 1).Date, new DateTime(2025, 12, 31).Date);
                foreach (Event item in calendarItemsRes)
                {
                    //log.LogEverything(t.GetMethodName("OutlookOnlineController"), "OutlookOnlineController.AppointmentItemFind calendarItemsRes current item.Id is " + item.Id);
                    if (item.Id.Equals(globalId))
                        return item;
                }

//                log.LogEverything(t.GetMethodName("OutlookOnlineController"), "No match found for " + nameof(globalId) + ":" + globalId);
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(_t.GetMethodName("OutlookOnlineController") + " failed. Due to no match found global id:" + globalId, ex);
            }
        }

        private string AppointmentPrint(Event appItem)
        {
            return "GlobalId:" + appItem.Id + " / Start:" + appItem.Start + " / Title:" + appItem.Subject;
        }

        private List<Event> GetCalendarItems(DateTime tLimitFrom, DateTime tLimitTo)
        {
            lock (_lockOutlook)
            {
//                log.LogEverything(t.GetMethodName("OutlookOnlineController"), "OutlookOnlineController.GetCalendarItems called");
                string filter = "GetCalendarItems [After] '" + tLimitTo.ToString("g") + "' AND [before] <= '" + tLimitFrom.ToString("g") + "'";
//                log.LogVariable(t.GetMethodName("OutlookOnlineController"), nameof(filter), filter.ToString());
                _calendarName = GetCalendarName();
                _userEmailAddress = GetUserEmailAddress();
                CalendarList calendarList = _outlookExchangeOnlineApiClient.GetCalendarList(_userEmailAddress, _calendarName);
                if (calendarList != null)
                {
                    foreach (Calendar cal in calendarList.value)
                    {
//                        log.LogEverything(t.GetMethodName("OutlookOnlineController"), "GetCalendarItems comparing cal.Name " + cal.Name + " with calendarName " + calendarName);
                        if (cal.Name.Equals(_calendarName, StringComparison.OrdinalIgnoreCase))
                        {
                            EventList outlookCalendarItems = _outlookExchangeOnlineApiClient.GetCalendarItems(_userEmailAddress, cal.Id, tLimitFrom, tLimitTo);
                            //log.LogVariable(t.GetMethodName("OutlookOnlineController"), "outlookCalendarItems.Count", outlookCalendarItems.value.Count);
                            return outlookCalendarItems.value;
                        }
                    }
                }
                return null;

            }
        }

        private string GetCalendarName()
        {
//            log.LogEverything(t.GetMethodName("OutlookOnlineController"), "GetCalendarName called");
            if (!string.IsNullOrEmpty(_calendarName))
                return _calendarName;
            else
            {
                _calendarName = _dbContext.PluginConfigurationValues
                    .SingleOrDefault(x => x.Name == "AppointmentBaseSettings:calendarName")?.Value;
                if (!string.IsNullOrEmpty(_calendarName))
                    return _calendarName;
                else
                    throw new Exception(_t.GetMethodName("OutlookOnlineController") + " failed, to get calendarName'");
            }

        }
        public string GetUserEmailAddress()
        {
//            log.LogEverything(t.GetMethodName("OutlookOnlineController"), "GetUserEmailAddress called");
            if (!string.IsNullOrEmpty(_userEmailAddress))
                return _userEmailAddress;
            else
            {
                _userEmailAddress = _dbContext.PluginConfigurationValues
                    .SingleOrDefault(x => x.Name == "AppointmentBaseSettings:userEmailAddress")?.Value;;
                if (!string.IsNullOrEmpty(_userEmailAddress))
                    return _userEmailAddress;
                else
                    throw new Exception(_t.GetMethodName("OutlookOnlineController") + " failed, to get userEmailAddess");
            }
        }
        private string GetCalendarID()
        {
            _calendarName = GetCalendarName();
            _userEmailAddress = GetUserEmailAddress();
            CalendarList calendarList = _outlookExchangeOnlineApiClient.GetCalendarList(_userEmailAddress, _calendarName);
            if (calendarList != null)
            {
                foreach (Calendar cal in calendarList.value)
                {
                    if (cal.Name.Equals(_calendarName, StringComparison.OrdinalIgnoreCase))
                    {
                        return cal.Id;
                    }
                }
            }
            return null;
        }

        #endregion


        private string CalendarItemUpdateBody(string BodyContent, string Location, string Category)
        {
            TimeZone localZone = TimeZone.CurrentTimeZone;
            string UpdateBody = "{\"Location\": {\"DisplayName\": \"" + Location + "\"},\"Body\": {\"ContentType\": \"HTML\",\"Content\": \"" + ReplaceLinesInBody(BodyContent) + "\"},\"Categories\": [\"" + Category + "\"]}";
            return UpdateBody;
        }
        private string CalendarItemCreateBody(string Subject, string BodyContent, string Location, DateTime Start, DateTime End, string originalStartTimeZone, string originalEndTimeZone)
        {
//            log.LogEverything(t.GetMethodName("OutlookOnlineController"), "CalendarItemCreateBody called and creating with start : " + Start + " and end : " + End);
//            log.LogEverything(t.GetMethodName("OutlookOnlineController"), "CalendarItemCreateBody called and creating with originalStartTimeZone : " + originalStartTimeZone + " and originalEndTimeZone : " + originalEndTimeZone);

            TimeZone localZone = TimeZone.CurrentTimeZone;
            string CreateBody = "{\"Subject\": \"" + Subject + "\",\"Body\": {\"ContentType\": \"HTML\",\"Content\": \"" + ReplaceLinesInBody(BodyContent) + "\"},\"Location\": {\"DisplayName\": \"" + Location + "\"},\"Start\": {\"DateTime\": \"" + Start.ToLocalTime() + "\",\"TimeZone\": \"" + localZone.StandardName + "\"},\"End\":{\"DateTime\":\"" + End.ToLocalTime() + "\",\"TimeZone\": \"" + localZone.StandardName + "\"}}";
            return CreateBody;
        }
        private string ReplaceLinesInBody(string BodyPreview)
        {
            return BodyPreview.Replace("\r\n", "<br/>"); ;
        }
        #endregion
    }

    public enum CalendarItemCategory
    {
        Completed,
        Error,
        Processing,
        Retrived,
        Revoked,
        Sent
    }
}