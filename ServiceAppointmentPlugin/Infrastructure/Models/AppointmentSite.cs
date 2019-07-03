namespace ServiceAppointmentPlugin.Infrastructure.Models
{
    public class AppointmentSite
    {
        #region var/pop
        public int? Id { get; set; }
        public int MicrotingSiteUid { get; set; }
        public string ProcessingState { get; set; }
        public string SdkCaseId { get; set; }
        #endregion
    }
}
