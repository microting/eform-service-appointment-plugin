using System;
using System.Collections.Generic;
using System.Linq;
using eFormShared;
using Microting.AppointmentBase.Infrastructure.Data;
using Microting.AppointmentBase.Infrastructure.Data.Entities;
using Constants = Microting.AppointmentBase.Infrastructure.Data.Constants.Constants;

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
        
        

        public static Microting.AppointmentBase.Infrastructure.Data.Entities.Appointment AppointmentsFindOne(
            AppointmentPnDbContext dbContext, int timesReflected)
        {
            try
            {
                var match = dbContext.Appointments.FirstOrDefault(x => x.Completed == timesReflected);
                return match;
            }
            catch (Exception ex)
            {
//                log.LogException(t.GetMethodName("SQLController"), "failed", ex, false);
                return null;
            }
        }
        
        public static bool AppointmentsUpdate(AppointmentPnDbContext dbContext, string globalId,
            ProcessingStateOptions processingState, string body, string exceptionString, string response,
            bool completed, DateTime start_at, DateTime expire_at, int duration)
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
                match.Duration = duration;
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
                    match.ExceptionString = exceptionString;
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
        
        public static bool AppointmentsReflected(AppointmentPnDbContext dbContext, string globalId)
        {
            try
            {
                    var match = dbContext.Appointments.SingleOrDefault(x => x.GlobalId == globalId);

                    if (match == null)
                        return false;

                    short? temp = match.Completed;

                    if (match.Completed == 0)
                        temp = 1;

                    if (match.Completed == 1)
                        temp = 2;

                    match.Completed = temp;
                    match.Update(dbContext);

                    return true;
            }
            catch (Exception ex)
            {
//                log.LogException(t.GetMethodName("SQLController"), "failed", ex, false);
                return false;
            }
        }
        
        public static Appointment AppointmentFindByCaseId(AppointmentPnDbContext dbContext, string sdkCaseId)
        {
            try
            {
//                using (var db = GetContextO())
//                {
                Microting.AppointmentBase.Infrastructure.Data.Entities.AppointmentSite appointmentSite = dbContext.AppointmentSites.SingleOrDefault(x => x.SdkCaseId == sdkCaseId);

                if (appointmentSite == null)
                    return null;

                Microting.AppointmentBase.Infrastructure.Data.Entities.Appointment _appo = appointmentSite.Appointment;
                Appointment appointment = new Appointment()
                {
                    
                };
//                Appointment appo = new Appointment(_appo.global_id, (DateTime)_appo.start_at, (int)_appo.duration, _appo.subject, _appo.processing_state, _appo.body, (_appo.color_rule == 0 ? false : true), _appo.id);
//                AppoinntmentSite appo_site = new AppoinntmentSite((int)_appo_site.id, _appo_site.microting_site_uid, _appo_site.processing_state, _appo_site.sdk_case_id);
//                appo.AppointmentSites.Add(appo_site);

                return appointment;
//                }
            }
            catch (Exception ex)
            {
//                log.LogException(t.GetMethodName("SQLController"), "failed", ex, false);
                return null;
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
        
        public void ParseBodyContent(eFormCore.Core sdkCore)
        {

            FindCheckListId();
            FindSites();
            FindFieldPreFillValues(sdkCore);


        }
//        #endregion

        #region private
        private void FindCheckListId()
        {
            TemplateId = int.Parse(FindValue("template#"));
        }

        private void FindSites()
        {
            string[] lines = Body.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string check = "sites#";
            foreach (var line in lines)
            {
                string input = line.ToLower();

                if (input.Contains(check))
                {
                    // Extracts the content to the right of the searchkey.
                    string lineNoComma = line.Remove(0, check.Length).Trim();
                    lineNoComma = lineNoComma.Replace(",", "|");

                    foreach (var item in t.TextLst(lineNoComma))
                    {
                        AppointmentSite appointmentSite = new AppointmentSite()
                        {
                            MicrotingSiteUid = int.Parse(item),
                            ProcessingState = Constants.ProcessingState.Processed
                        };
                        AppointmentSites.Add(appointmentSite);
                    }

                    AppointmentSites = AppointmentSites.Distinct().ToList();

                    continue;
                }
            }
        }

        private string FindValue(string searchKey)
        {
            string[] lines = Body.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string returnValue = "";
            foreach (var line in lines)
            {
                string input = line.ToLower();
                if (input.Trim() == "")
                    continue;
                if (input.Contains(searchKey.ToLower()))
                {
                    // Extracts the content to the right of the searchkey.
                    string itemStr = line.Remove(0, searchKey.Length).Trim();

                    returnValue = itemStr;

                    continue;
                }
            }
            return returnValue;
        }
        
        private void FindFieldPreFillValues(eFormCore.Core sdkCore)
        {
            List<Field_Dto> fieldDtos = sdkCore.Advanced_TemplateFieldReadAll(TemplateId);

            foreach (Field_Dto fieldDto in fieldDtos)
            {
                string value = FindValue($"F{fieldDto.Id}#");

                if (!string.IsNullOrEmpty(value))
                {
                    AppointmentPrefillFieldValue appointmentPrefillFieldValue = new AppointmentPrefillFieldValue()
                    {
                        FieldId = fieldDto.Id,
                        FieldValue = value
                    };
                    AppointmentPrefillFieldValues.Add(appointmentPrefillFieldValue);
                }
            }
        }
        #endregion
    }
}