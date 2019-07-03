using System;
using ServiceAppointmentPlugin.OfficeApi;

namespace ServiceAppointmentPlugin.Messages
{

    public class ParseOutlookItem
    {
        public Event Item { get; protected set; }

        public ParseOutlookItem(Event item)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
        }
    }
}
