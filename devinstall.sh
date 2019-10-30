#!/bin/bash

cd ~

if [ -d "Documents/workspace/microting/eform-debian-service/Plugins/ServiceAppointmentPlugin" ]; then
	rm -fR Documents/workspace/microting/eform-debian-service/Plugins/ServiceAppointmentPlugin
fi

cp -av Documents/workspace/microting/eform-service-appointment-plugin/ServiceAppointmentPlugin Documents/workspace/microting/eform-debian-service/Plugins/ServiceAppointmentPlugin
