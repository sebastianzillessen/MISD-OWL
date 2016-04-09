required packages (yum install <PACKAGE>):

mono-core-2.10.8-2.fc17
mono-reflection-0.1-0.4.20110613git304d1d.fc17
mono-wcf-2.10.8-2.cf17
lm_sensors


Please verify that your sensors are detected (sudo sensors-detect), to check this you can use the command "sensors".

You must adapt your Webservice-Url to the correct one in the 
	
	MISD.Workstation.Linux.exe.config  
File. Please change the endpoint-address to your specified address in the server-configuration:

	<client>
            <endpoint address="http://acid.visus.uni-stuttgart.de:8002/workstationWebService"
                binding="basicHttpBinding" bindingConfiguration="BasicHttpBinding_IWorkstationWebService"
                contract="WSWebService.IWorkstationWebService" name="BasicHttpBinding_IWorkstationWebService" />
        </client>

Change 
	http://acid.visus.uni-stuttgart.de:8002/workstationWebService
to your servers address.
