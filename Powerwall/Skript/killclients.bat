@ECHO OFF
SET Pskill=\\visus\vestastore\demos\MISD\pskill.exe
SET Machines=keshiki01 keshiki02 keshiki03 keshiki04 keshiki05 keshiki06 keshiki07 keshiki08 keshiki09 keshiki10
REM SET Machines=keshiki01 keshiki02 keshiki03 keshiki04 keshiki05 keshiki06 keshiki07 keshiki11 keshiki09 keshiki10

@ECHO ON
FOR %%M IN (%Machines%) DO cmd /C %Pskill% \\%%M misd.client.exe
