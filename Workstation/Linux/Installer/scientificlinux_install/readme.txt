Run the following code from a root-shell:

yum install -Y gcc libtool bison pkg-config libglib2.0-dev gettext make bzip2 g++ gcc-c++

wget http://origin-download.mono-project.com/sources/mono/mono-2.10.2.tar.bz2
tar xvjf mono-2.10.2.tar.bz2
cd mono-2.10.2
./configure --prefix=/opt/mono-2.10

# this might take 30-60 minutes
make
make install

cd /usr/bin
mv mono mono.old
mv gmcs gmcs.old
ln -s /opt/mono-2.10/bin/mono /usr/bin/mono
ln -s /opt/mono-2.10/bin/gmcs /usr/bin/gmcs

Check now your Mono-Version with

mono -V

it should return something like 2.10.2


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
