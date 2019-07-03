using System.Collections.Generic;

namespace ServiceAppointmentPlugin.OfficeApi
{
    public class CalendarList
    {
        public List<Calendar> value { get; set; }
    }

    public class Calendar
    {
        public bool CanEdit { get; set; }
        public bool CanShare { get; set; }
        public bool CanViewPrivateItems { get; set; }
        public string ChangeKey { get; set; }
        public string Color { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public CalendarOwner Owner { get; set; }
    }
    public class CalendarOwner
    {
        public string Address { get; set; }
        public string Name { get; set; }
    }
}