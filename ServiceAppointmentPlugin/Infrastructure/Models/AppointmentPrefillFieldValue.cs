namespace ServiceAppointmentPlugin.Infrastructure.Models
{
    public class AppointmentPrefillFieldValue
    {
        #region var/prop
        public int Id { get; set; }
        public int FieldId { get; set; }
        public string FieldValue { get; set; }
        public int AppointmentId { get; set; }

        #endregion
    }
}
