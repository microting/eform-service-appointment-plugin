using System;
using System.Collections.Generic;
using System.Linq;
using Microting.AppointmentBase.Infrastructure.Data;
using Microting.AppointmentBase.Infrastructure.Data.Entities;

namespace ServiceAppointmentPlugin.Infrastructure.Models
{
    public class Appointment
    {
        
        #region var/pop
        public string GlobalId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int Duration { get; set; }
        public int? Id { get; set; }
        public string Subject { get; set; }
        public string ProcessingState { get; set; }
        public string Body { get; set; }
        public bool Completed { get; set; }

        public int TemplateId { get; set; }
        public List<AppointmentSite> AppointmentSites { get; set; }
        public List<AppointmentPrefillFieldValue> AppointmentPrefillFieldValues { get; set; }
        public bool Connected { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Info { get; set; }
        public List<string> Replacements { get; set; }
        public int Expire { get; set; }
        public bool ColorRule { get; set; }
        public string MicrotingUId { get; set; }

        Tools t = new Tools();
        #endregion
        
        public static Appointment AppointmentsFind(AppointmentPnDbContext dbContext, string globalId)
        {
//            log.LogStandard(t.GetMethodName("SQLController"), "AppointmentsFind looking for one with globalId " + globalId);

            try
            {
                    var match = dbContext.Appointments.SingleOrDefault(x => x.GlobalId == globalId);
                    if (match != null)
                    {
                        bool color_rule = match.ColorRule == 0 ? true : false;
                        Appointment appo = new Appointment()
                        {
                            GlobalId = match.GlobalId,
                            Start = (DateTime)match.StartAt,
                            Duration = (int)match.Duration,
                            Subject = match.Subject,
                            ProcessingState = match.ProcessingState,
                            Body = match.Body,
                            ColorRule = color_rule,
                            Id = match.Id
                        };
                        appo.Completed = match.Completed == 0 ? false : true;
                        try {
                            appo.TemplateId = (int)match.SdkeFormId;
                        }
                        catch { }

                        foreach (Microting.AppointmentBase.Infrastructure.Data.Entities.AppointmentSite appointmentSite in match.AppointmentSites)
                        {
                            AppointmentSite obj = new AppointmentSite()
                            {
                                Id = appointmentSite.Id,
                                MicrotingSiteUid = appointmentSite.MicrotingSiteUid,
                                ProcessingState = appointmentSite.ProcessingState,
                                SdkCaseId = appointmentSite.SdkCaseId
                            };
                            appo.AppointmentSites.Add(obj);
                        }

                        foreach (Microting.AppointmentBase.Infrastructure.Data.Entities.AppointmentPrefillFieldValue pfv in match.AppointmentPrefillFieldValues)
                        {
                            AppointmentPrefillFieldValue appointmentPrefillFieldValue = new AppointmentPrefillFieldValue()
                            {
                                Id = pfv.Id,
                                FieldId = pfv.FieldId,
                                AppointmentId = (int)pfv.AppointmentId,
                                FieldValue = pfv.FieldValue
                            };
                            appo.AppointmentPrefillFieldValues.Add(appointmentPrefillFieldValue);
                        }
                        return appo;
                    }
                    else
                    {
                        return null;
                    }
            }
            catch (Exception ex)
            {
//                log.LogException(t.GetMethodName("SQLController"), "failed", ex, false);
                return null;
            }
        }
        
        public static bool AppointmentsUpdate(AppointmentPnDbContext dbContext, string globalId, ProcessingStateOptions processingState, string body, string expectionString, string response, bool completed, DateTime start_at, DateTime expire_at, int durateion)
        {
//            log.LogEverything(t.GetMethodName("SQLController"), "AppointmentsUpdate called and globalId is " + globalId);

            try
            {
                var match = dbContext.Appointments.SingleOrDefault(x => x.GlobalId == globalId);

                if (match == null)
                    return false;

                match.ProcessingState = processingState.ToString();
//                match.updated_at = DateTime.Now;
                match.StartAt = start_at;
                match.ExpireAt = expire_at;
                match.Duration = durateion;
                //match.completed = 0;
                #region match.body = body ...
                if (body != null)
                    match.Body = body;
                #endregion
                #region match.response = response ...
                if (response != null)
                    match.Response = response;
                #endregion
                #region match.expectionString = expectionString ...
                if (response != null)
                    match.ExceptionString = expectionString;
                #endregion
//                match.version = match.version + 1;

                match.Update(dbContext);

                return true;
            }
            catch (Exception ex)
            {
//                log.LogException(t.GetMethodName("SQLController"), "failed", ex, false);
                return false;
            }
        }
        
        public enum ProcessingStateOptions
        {
            //Appointment locations options / ProcessingState options
            Pre_created,
            Planned,
            Processed,
            Created,
            Sent,
            Retrived,
            Completed,
            Canceled,
            Revoked,
            ParsingFailed,
            Exception,
            Unknown_location
        }
    }
}