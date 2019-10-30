#!/bin/bash

cd ~

if [ -d "Documents/workspace/microting/eform-service-appointment-plugin/ServiceAppointmentPlugin" ]; then
	rm -fR Documents/workspace/microting/eform-service-trashinspection-plugin/ServiceAppointmentPlugin
fi

cp -av Documents/workspace/microting/eform-debian-service/Plugins/ServiceAppointmentPlugin Documents/workspace/microting/eform-service-appointment-plugin/ServiceAppointmentPlugin
