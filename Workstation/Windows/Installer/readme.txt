1. Install

- Start MISD.Workstation.Windows Setup.msi or the setup.exe.
- Follow the installation guide.
- Specify the destination folder during the install process.
default location: C:\Program Files (x86)\MISD\
- Finish the install process.

Once successful installed, the MISD service will be started automatically.


2. View log

The program folder contains a log file which provides information about past actions.
default location: C:\Program Files (x86)\MISD\log.txt


3. Change server URL

- Stop the service, therefor go to the Windows service list.
- Search for MISD Workstation Service and stop it.
- Browse to the MISD program folder and copy MISD.Workstation.Windows.exe.config to your desktop.
- Edit the service endpoint adress. default adress:
<client>
   <endpoint address="http://acid.visus.uni-stuttgart.de:8002/workstationWebService"...
- Save changes and overwrite MISD.Workstation.Windows.exe.config in the program folder.
- Admin privileges may be neeeded.
- Restart MISD Workstation Service in the Windows services. 
