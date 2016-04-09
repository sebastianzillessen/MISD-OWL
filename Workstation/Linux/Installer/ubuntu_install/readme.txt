required packages (apt-get install <PACKAGE>):

mono-complete
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
