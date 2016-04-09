#!/bin/bash
#-------------------------------------------------------------------------------------
# Copyright 2012 Paul Brombosch, Ehssan Doust, David Krauss,
# Fabian Müller, Yannic Noller, Hanna Schäfer, Jonas Scheurich,
# Arno Schneider, Sebastian Zillessen
#
# This file is part of MISD-OWL, a project of the
# University of Stuttgart (Institution VISUS, Studienprojekt Spring 2012).
#
# MISD-OWL is published under GNU Lesser General Public License Version 3.
# MISD-OWL is free software, you are allowed to redistribute and/or
# modify it under the terms of the GNU Lesser General Public License
# Version 3 or any later version. For details see here:
# http://www.gnu.org/licenses/lgpl.html
#
# MISD-OWL is distributed without any warranty, without even the
# implied warranty of merchantability or fitness for a particular purpose.
#-------------------------------------------------------------------------------------
# Author: MISD OWL Team
# Version: 1.00
#-------------------------------------------------------------------------------------
# Description:
# This batch file installs the MISD OWL WorkstationService on a Linux
# Workstation.
# This file is recommended for the distribution Ubuntu 12.04 LTS.
#-------------------------------------------------------------------------------------
ROOT_UID="0"

#Check if run as root
if [ "$UID" -ne "$ROOT_UID" ] ; then
	echo "You must be root to do that!"
	exit 1
fi
echo "Installing MISD WorkstationService on Ubuntu OS..."

echo "Registering misd daemon script..."
#Write start script
START_FILE="/etc/init.d/misd"
cp misd $START_FILE
chmod a+rx $START_FILE
#set links for boot and shutdown (fedora)
update-rc.d misd defaults

echo "Copying files..."
# We are root, so start copying files now
cd ..
if [ -d "/opt/misd/" ]; then
	echo "Installation was already present. Deleting it..."
    rm "/opt/misd/" -r -f
    echo "done."
fi

echo "Installing files..."
mkdir "/opt/"
mkdir "/opt/misd/"
cp MISD.Core.dll MISD.Core.dll.mdb MISD.Workstation.Linux.exe MISD.Workstation.Linux.exe.config MISD.Workstation.Linux.exe.mdb MISD.RegExUtil.dll /opt/misd/
mkdir "/opt/misd/plugins/"

echo "Done!"
