namespace ServiceAppointmentPlugin.Messages
{
    public class EformCompleted
    {
        public string caseId { get; protected set; }

        public EformCompleted(string caseId)
        {
            this.caseId = caseId;
        }
    }
}
