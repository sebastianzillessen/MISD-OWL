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
# This batch file uninstalls the MISD OWL WorkstationService from
# a Linux Workstation.
# This file is recommended for the distribution Fedora 17.
#-------------------------------------------------------------------------------------
ROOT_UID="0"

#Check if run as root
if [ "$UID" -ne "$ROOT_UID" ] ; then
	echo "You must be root to do that!"
	exit 1
fi
echo "Deleting MISD Files..."
#Remove links for boot and shutdown (fedora)
chkconfig --del misd
#Delete MISD Files
rm "/opt/misd/" -r -f
rm "/etc/init.d/misd" -f


echo "Done!"
