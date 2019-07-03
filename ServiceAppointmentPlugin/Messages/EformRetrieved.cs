namespace ServiceAppointmentPlugin.Messages
{
    public class EformRetrieved
    {
        public string caseId { get; protected set; }

        public EformRetrieved(string caseId)
        {
            this.caseId = caseId;
        }
    }
}
