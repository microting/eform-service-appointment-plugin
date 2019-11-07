#!/bin/bash

if [ ! -d "/var/www/microting/eform-service-appointment-plugin" ]; then
  cd /var/www/microting
  su ubuntu -c \
  "git clone https://github.com/microting/eform-service-appointment-plugin.git -b stable"
fi

cd /var/www/microting/eform-service-appointment-plugin
su ubuntu -c \
"dotnet restore ServiceAppointmentPlugin.sln"

echo "################## START GITVERSION ##################"
export GITVERSION=`git describe --abbrev=0 --tags | cut -d "v" -f 2`
echo $GITVERSION
echo "################## END GITVERSION ##################"
su ubuntu -c \
"dotnet publish ServiceAppointmentPlugin.sln -o out /p:Version=$GITVERSION --runtime linux-x64 --configuration Release"

su ubuntu -c \
"mkdir -p /var/www/microting/eform-debian-service/MicrotingService/MicrotingService/out/Plugins/"

su ubuntu -c \
"cp -av /var/www/microting/eform-service-itemsplanning-plugin/ServiceAppointmentPlugin/out /var/www/microting/eform-debian-service/MicrotingService/MicrotingService/out/Plugins/ServiceAppointmentPlugin"
/rabbitmqadmin declare queue name=eform-service-appointment-plugin durable=true
