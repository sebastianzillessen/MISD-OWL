#$Machines = @( "keshiki01", "keshiki02", "keshiki03", "keshiki04", "keshiki05", "keshiki06", "keshiki07", "keshiki08", "keshiki09", "keshiki10" )
#$Machines = @( "keshiki01", "keshiki02", "keshiki03", "keshiki04", "keshiki05", "keshiki06", "keshiki07", "keshiki11", "keshiki09", "keshiki10" )
$Machines = @( "keshiki01", "keshiki03", "keshiki05", "keshiki07", "keshiki09" )

#$Machines = @( "keshiki01", "keshiki03")
$User = "visus\vesta-interactive"
$Pass = "h1rzP1a"

cd (split-path -parent $MyInvocation.MyCommand.Definition)

foreach ($m in $Machines) {
    $id = [int]($m-replace("keshiki", ""))
    $offset = [System.Math]::Floor($id / 2) * -2100
    $params = @("\\$m", "-i", "-u", $User, "-p", $Pass, "-f", "-c" ,"client.bat", $offset)
    start .\psexec.exe $params
}
