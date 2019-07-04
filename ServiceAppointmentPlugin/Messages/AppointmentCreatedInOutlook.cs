using System;
using ServiceAppointmentPlugin.Infrastructure.Models;

namespace ServiceAppointmentPlugin.Messages
{

    public class AppointmentCreatedInOutlook
    {
        public Appointment Appo { get; protected set; }

        public AppointmentCreatedInOutlook(Appointment appo)
        {
            Appo = appo ?? throw new ArgumentNullException(nameof(appo));
        }
    }
}
