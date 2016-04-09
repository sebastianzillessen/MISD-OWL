net use /delete v:
net use /persistent:no v: \\visus\vestastore\demos

V:
xcopy V:\MISD\debug C:\temp\misd\ /Y /S /E


C:
cd c:\temp\misd
misd.client.exe powerwall 192.168.219.123 %1
